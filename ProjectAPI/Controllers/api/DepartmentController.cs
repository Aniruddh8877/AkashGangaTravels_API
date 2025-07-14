using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Department")]
    public class DepartmentController : ApiController
    {

        [HttpPost]
        [Route("DepartmentList")]
        public ExpandoObject DepartmentList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Department model = JsonConvert.DeserializeObject<Department>(decryptData);

                var list = (from d1 in dbContext.Departments
                           where (model.DepartmentId == d1.DepartmentId || model.DepartmentId == 0)
                           && (model.Status == d1.Status || model.Status == 0)
                           orderby d1.DepartmentName
                           select new
                           {
                               d1.DepartmentId,
                               d1.DepartmentName,
                               d1.Status,
                           }).ToList();

                response.DepartmentList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveDepartment")]
        public ExpandoObject SaveDepartment(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Department model = JsonConvert.DeserializeObject<Department>(decryptData);

                Department Department = null;
                if (model.DepartmentId > 0)
                {
                    Department = dbContext.Departments.Where(x => x.DepartmentId == model.DepartmentId).First();
                    Department.DepartmentName = model.DepartmentName;
                    Department.Status = model.Status;
                }
                else
                    Department = model;

                if (Department.DepartmentId == 0)
                    dbContext.Departments.Add(Department);
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
        [Route("deleteDepartment")]
        public ExpandoObject DeleteDepartment(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Department model = JsonConvert.DeserializeObject<Department>(decryptData);

                var Department = dbContext.Departments.Where(x => x.DepartmentId == model.DepartmentId).First();
                dbContext.Departments.Remove(Department);
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
