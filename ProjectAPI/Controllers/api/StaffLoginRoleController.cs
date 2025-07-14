using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/StaffLoginRole")]

    public class StaffLoginRoleController : ApiController
    {
        [HttpPost]
        [Route("StaffLoginRoleList")]
        public ExpandoObject StaffLoginRoleList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                //StaffLoginRole model = JsonConvert.DeserializeObject<StaffLoginRole>(decryptData);
                var list = (from d1 in dbContext.StaffLoginRoles
                                //where (model.StaffLoginRoleId == d1.StaffLoginRoleId || model.StaffLoginRoleId == 0)
                            join d2 in dbContext.StaffLogins on d1.StaffLoginId equals d2.StaffLoginId
                            join d3 in dbContext.Roles on d1.RoleId equals d3.RoleId
                            orderby d1.StaffLoginId
                            select new
                            {
                                d1.StaffLoginRoleId,
                                d2.StaffLoginId,
                                d2.UserName,
                                d3.RoleId,
                                d3.RoleTitle
                            }).ToList();

                response.StaffLoginRoleList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveStaffLoginRole")]
        public ExpandoObject SaveStaffLoginRole(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StaffLoginRole model = JsonConvert.DeserializeObject<StaffLoginRole>(decryptData);

                StaffLoginRole StaffLoginRole = null;
                if (model.StaffLoginRoleId > 0)
                {
                    StaffLoginRole = dbContext.StaffLoginRoles.Where(x => x.StaffLoginRoleId == model.StaffLoginRoleId).First();
                    StaffLoginRole.UpdatedBy = model.UpdatedBy;
                    StaffLoginRole.StaffLoginId = model.StaffLoginId;
                    StaffLoginRole.RoleId = model.RoleId;
                }
                else
                    StaffLoginRole = model;

                StaffLoginRole.UpdatedDate = DateTime.Now;

                if (StaffLoginRole.StaffLoginRoleId == 0)
                    dbContext.StaffLoginRoles.Add(StaffLoginRole);
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
        [Route("deleteStaffLoginRole")]
        public ExpandoObject DeleteStaffLoginRole(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StaffLoginRole model = JsonConvert.DeserializeObject<StaffLoginRole>(decryptData);
                var StaffLoginRole = dbContext.StaffLoginRoles.Where(x => x.StaffLoginRoleId == model.StaffLoginRoleId).First();
                dbContext.StaffLoginRoles.Remove(StaffLoginRole);
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
