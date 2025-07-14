using Newtonsoft.Json;
using Project;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Menu")]
    public class MenuController : ApiController
    {

        [HttpPost]
        [Route("MenuList")]
        public ExpandoObject MenuList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                // Menu model = JsonConvert.DeserializeObject<Menu>(decryptData);

                var list = (from d1 in dbContext.Menus
                            join d2 in dbContext.Pages on d1.PageId equals d2.PageId into subPages
                            from sPage in subPages.DefaultIfEmpty()
                            where d1.ParentMenuId == null
                            && d1.Status == (byte)Status.Active
                            orderby d1.MenuNo
                            select new
                            {
                                d1.MenuId,
                                d1.PageId,
                                sPage.PageName,
                                d1.MenuTitle,
                                d1.MenuNo,
                                d1.ParentMenuId,
                                d1.MenuIcon,
                                d1.Status,
                                MenuList = (from t1 in dbContext.Menus
                                            join t2 in dbContext.Pages on t1.PageId equals t2.PageId
                                            where t1.ParentMenuId == d1.MenuId
                                            && d1.Status == (byte)Status.Active
                                            orderby t1.MenuNo
                                            select new
                                            {
                                                t1.MenuId,
                                                t1.PageId,
                                                t2.PageName,
                                                t1.MenuTitle,
                                                t1.MenuNo,
                                                t1.ParentMenuId,
                                                t1.MenuIcon,
                                                t1.Status,
                                            }).ToList()
                            }).ToList();

                response.MenuList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveMenu")]
        public ExpandoObject SaveMenu(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Menu model = JsonConvert.DeserializeObject<Menu>(decryptData);

                Menu Menu = null;
                if (model.MenuId > 0)
                {
                    Menu = dbContext.Menus.Where(x => x.MenuId == model.MenuId).First();
                    Menu.UpdatedBy = model.UpdatedBy;
                    Menu.PageId = model.PageId;
                    Menu.MenuIcon = model.MenuIcon;
                    Menu.ParentMenuId = model.ParentMenuId;
                    Menu.MenuTitle = model.MenuTitle;
                    Menu.Status = model.Status;
                }
                else
                {
                    Menu = model;
                    var preMenu = dbContext.Menus.Where(x => x.ParentMenuId == model.ParentMenuId);
                    if (preMenu.Any())
                        Menu.MenuNo = preMenu.OrderByDescending(x => x.MenuNo).First().MenuNo + 1;
                    else
                        Menu.MenuNo = 1;
                }
                Menu.UpdatedDate = DateTime.Now;
                if (Menu.MenuId == 0)
                    dbContext.Menus.Add(Menu);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("IX"))
                    response.Message = "This record is already exist";
                else
                    response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("deleteMenu")]
        public ExpandoObject DeleteMenu(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Menu model = JsonConvert.DeserializeObject<Menu>(decryptData);
                var Menu = dbContext.Menus.Where(x => x.MenuId == model.MenuId).First();
                dbContext.Menus.Remove(Menu);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                    response.Message = "This record is in use.so can't delete.";
                else
                    response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("MenuUp")]
        public ExpandoObject MenuUp(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
            DbContextTransaction transaction = null;
            int? y = null;
            try
            {
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Menu model = JsonConvert.DeserializeObject<Menu>(decryptData);

                transaction = dbContext.Database.BeginTransaction();
                y = 0;
                var Menu = dbContext.Menus.Where(x => x.MenuId == model.MenuId).First();
                int menuNo = Menu.MenuNo;
                var preMenu = dbContext.Menus.Where(x => (Menu.ParentMenuId.HasValue ? x.ParentMenuId == Menu.ParentMenuId : x.ParentMenuId == null) && x.MenuNo < Menu.MenuNo).OrderByDescending(x => x.MenuNo).First();
                Menu.MenuNo = preMenu.MenuNo;
                dbContext.SaveChanges();

                preMenu.MenuNo = menuNo;
                dbContext.SaveChanges();

                transaction.Commit();
                y = 1;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (y == 0)
                    transaction.Rollback();
                response.Message = ex.Message;
            }
            
            return response;
        }

        [HttpPost]
        [Route("MenuDown")]
        public ExpandoObject MenuDown(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
            DbContextTransaction transaction = null;
            int? y = null;
            try
            {
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Menu model = JsonConvert.DeserializeObject<Menu>(decryptData);
                transaction = dbContext.Database.BeginTransaction();
                y = 0;
                var Menu = dbContext.Menus.Where(x => x.MenuId == model.MenuId).First();
                int menuNo = Menu.MenuNo;

                var preMenu = dbContext.Menus.Where(x => (Menu.ParentMenuId.HasValue ? x.ParentMenuId == Menu.ParentMenuId : x.ParentMenuId == null) && x.MenuNo > Menu.MenuNo).OrderBy(x => x.MenuNo).First();
                Menu.MenuNo = preMenu.MenuNo;
                dbContext.SaveChanges();

                preMenu.MenuNo = menuNo;
                dbContext.SaveChanges();

                transaction.Commit();
                y = 1;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (y ==0)
                    transaction.Rollback();
                response.Message = ex.Message;
            }
            
            return response;
        }

        [HttpPost]
        [Route("UserMenuList")]
        public ExpandoObject UserMenuList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StaffLogin model = JsonConvert.DeserializeObject<StaffLogin>(decryptData);

                var roleIds = dbContext.StaffLoginRoles.Where(x => x.StaffLoginId == model.StaffLoginId).Select(x => x.RoleId).ToList();
                var allMenuIds = dbContext.RoleMenus.Where(x1 => roleIds.Contains(x1.RoleId)).Select(x1 => x1.MenuId).Distinct().ToList();
                allMenuIds.AddRange(dbContext.Menus.Where(x => allMenuIds.Contains(x.MenuId)).Select(x => x.ParentMenuId ?? 0).Distinct().ToList());
                var menus = dbContext.Menus.Where(x1 => x1.Status == (byte)Status.Active && allMenuIds.Contains(x1.MenuId))
                    .Select(m1 => new MenuModel
                    {
                        MenuIcon = m1.MenuIcon,
                        MenuNo = m1.MenuNo,
                        MenuTitle = m1.MenuTitle,
                        PageId = m1.PageId,
                        ParentMenuId = m1.ParentMenuId,
                        MenuId = m1.MenuId,
                    }).ToList();
                var pageIds = menus.Select(x => x.PageId).ToList();
                var pages = dbContext.Pages.Where(x1 => pageIds.Contains(x1.PageId)).ToList();

                menus.ForEach(x1 => x1.PageUrl = x1.PageId.HasValue ? pages.First(x => x.PageId == x1.PageId).PageUrl : null);
                List<MenuModel> MenuList = menus.Where(x => x.ParentMenuId == null).OrderBy(x => x.MenuNo).ToList();
                MenuList.ForEach(m1 => m1.MenuList = menus.Where(x => x.ParentMenuId == m1.MenuId).OrderBy(x => x.MenuNo).ToList());
                response.MenuList = MenuList;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public class ValidiateMenuModel
        {
            public string Url { get; set; }
            public int StaffLoginId { get; set; }
        }

        [HttpPost]
        [Route("ValidiateMenu")]
        public ExpandoObject ValidiateMenu(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ValidiateMenuModel model = JsonConvert.DeserializeObject<ValidiateMenuModel>(decryptData);

                var staffRoles = dbContext.StaffLoginRoles.Where(x => x.StaffLoginId == model.StaffLoginId).Select(x => x.RoleId).ToList();
                var menus = (from m1 in dbContext.RoleMenus
                             join m2 in dbContext.Menus on m1.MenuId equals m2.MenuId
                             join m3 in dbContext.Menus on m2.ParentMenuId equals m3.MenuId into subParentMenus
                             from sParentMenu in subParentMenus.DefaultIfEmpty()
                             join p1 in dbContext.Pages on m2.PageId equals p1.PageId
                             join r1 in dbContext.RoleMenus on m2.MenuId equals r1.MenuId
                             where staffRoles.Contains(m1.RoleId)
                             && p1.PageUrl == model.Url
                             select new
                             {
                                 m1.CanCreate,
                                 m1.CanDelete,
                                 m1.CanEdit,
                                 m2.MenuTitle,
                                 ParentMenuTitle = sParentMenu.MenuTitle,
                             });
                if (menus.Any())
                {
                    var menu = menus.First();
                    response.Action = menus.First();
                    response.Message = ConstantData.SuccessMessage;
                }
                else
                    response.Message = ConstantData.AccessDenied;

            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
