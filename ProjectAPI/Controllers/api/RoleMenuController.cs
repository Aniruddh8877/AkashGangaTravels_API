using Newtonsoft.Json;
using Project;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/RoleMenu")]
    public class RoleMenuController : ApiController
    {
        public class RoleMenuModel
        {
            public bool IsSelected { get; set; }
            public int RoleMenuId { get; set; }
            public int MenuId { get; set; }
            public string MenuTitle { get; set; }
            public bool CanEdit { get; set; }
            public bool CanDelete { get; set; }
            public bool CanCreate { get; set; }
        }

        [HttpPost]
        [Route("AllRoleMenuList")]
        public ExpandoObject AllRoleMenuList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Role model = JsonConvert.DeserializeObject<Role>(decryptData);

                var list = (from tg1 in dbContext.PageGroups
                           orderby tg1.PageGroupName
                           select new
                           {
                               tg1.PageGroupName,
                               tg1.PageGroupId,
                               RoleMenuList = (from tm1 in dbContext.Pages
                                              join m1 in dbContext.Menus
                                              on tm1.PageId equals m1.PageId
                                              join r1 in dbContext.RoleMenus.Where(x => x.RoleId == model.RoleId)
                                              on m1.MenuId equals r1.MenuId into RoleMenuList
                                              from RoleMenu in RoleMenuList.DefaultIfEmpty()
                                              where tm1.PageGroupId == tg1.PageGroupId
                                              && (model.Status == 0 || tm1.Status == model.Status)
                                              select new RoleMenuModel
                                              {
                                                  IsSelected = RoleMenu.RoleId != null ? true : false,
                                                  MenuId = m1.MenuId,
                                                  MenuTitle = m1.MenuTitle,
                                                  RoleMenuId = RoleMenu.RoleMenuId != null ? RoleMenu.RoleMenuId : 0,
                                                  CanEdit = RoleMenu.CanEdit != null ? RoleMenu.CanEdit : false,
                                                  CanDelete = RoleMenu.CanDelete != null ? RoleMenu.CanDelete : false,
                                                  CanCreate = RoleMenu.CanCreate != null ? RoleMenu.CanCreate : false,
                                              }).ToList()
                           }).ToList();
                response.AllRoleMenuList = list.Where(x => x.RoleMenuList.Any());
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public class SaveRoleMenuModel
        {
            public int RoleId { get; set; }
            public int StaffLoginId { get; set; }
            public List<RoleMenuModel> RoleMenuList { get; set; }
        }

        [HttpPost]
        [Route("saveRoleMenu")]
        public ExpandoObject SaveRoleMenu(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                SaveRoleMenuModel data = JsonConvert.DeserializeObject<SaveRoleMenuModel>(decryptData);

                data.RoleMenuList.ForEach(model =>
                {
                    if (model.RoleMenuId > 0 && !model.IsSelected)
                    {
                        var RoleMenu = dbContext.RoleMenus.Where(x => x.RoleMenuId == model.RoleMenuId).First();
                        dbContext.RoleMenus.Remove(RoleMenu);
                        dbContext.SaveChanges();
                    }
                    else if (model.IsSelected)
                    {
                        RoleMenu RoleMenu = null;
                        if (model.RoleMenuId > 0)
                            RoleMenu = dbContext.RoleMenus.Where(x => x.RoleMenuId == model.RoleMenuId).First();
                        else
                            RoleMenu = new RoleMenu
                            {
                                RoleId = data.RoleId,
                                MenuId = model.MenuId,
                            };
                        RoleMenu.UpdatedBy = data.StaffLoginId;
                        RoleMenu.UpdatedDate = DateTime.Now;
                        RoleMenu.CanDelete = model.CanDelete;
                        RoleMenu.CanEdit = model.CanEdit;
                        RoleMenu.CanCreate = model.CanCreate;
                        if (RoleMenu.RoleMenuId == 0)
                            dbContext.RoleMenus.Add(RoleMenu);
                        dbContext.SaveChanges();
                    }
                });

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
    }
}
