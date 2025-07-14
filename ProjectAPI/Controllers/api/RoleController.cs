using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Role")]
    public class RoleController : ApiController
    {
        [HttpPost]
        [Route("RoleList")]
        public ExpandoObject RoleList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Role model = JsonConvert.DeserializeObject<Role>(decryptData);
                var list = (from d1 in dbContext.Roles
                            where (model.RoleId == d1.RoleId || model.RoleId == 0)
                            && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.RoleTitle
                            select new
                            {
                                d1.RoleId,
                                d1.RoleTitle,
                                d1.Status,
                            }).ToList();

                response.RoleList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveRole")]
        public ExpandoObject SaveRole(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Role model = JsonConvert.DeserializeObject<Role>(decryptData);

                Role Role = null;
                if (model.RoleId > 0)
                {
                    Role = dbContext.Roles.Where(x => x.RoleId == model.RoleId).First();
                    Role.UpdatedBy = model.UpdatedBy;
                    Role.RoleTitle = model.RoleTitle;
                    Role.Status = model.Status;
                }
                else
                {
                    Role = model;
                }
                Role.UpdatedDate = DateTime.Now;
                if (Role.RoleId == 0)
                    dbContext.Roles.Add(Role);
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
        [Route("deleteRole")]
        public ExpandoObject DeleteRole(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Role model = JsonConvert.DeserializeObject<Role>(decryptData);
                var Role = dbContext.Roles.Where(x => x.RoleId == model.RoleId).First();
                dbContext.Roles.Remove(Role);
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
    }
}
