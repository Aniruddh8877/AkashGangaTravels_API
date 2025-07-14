using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Designation")]
    public class DesignationController : ApiController
    {
        [HttpPost]
        [Route("DesignationList")]
        public ExpandoObject DesignationList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Designation model = JsonConvert.DeserializeObject<Designation>(decryptData);

                var list = (from d1 in dbContext.Designations
                           where (model.DesignationId == d1.DesignationId || model.DesignationId == 0)
                           && (model.Status == d1.Status || model.Status == 0)
                           orderby d1.DesignationName
                            select new
                            {
                                d1.DesignationId,
                                d1.DesignationName,
                                d1.Status,
                            }).ToList();

                response.DesignationList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveDesignation")]
        public ExpandoObject SaveDesignation(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Designation model = JsonConvert.DeserializeObject<Designation>(decryptData);

                Designation Designation = null;
                if (model.DesignationId > 0)
                {
                    Designation = dbContext.Designations.Where(x => x.DesignationId == model.DesignationId).First();
                    Designation.DesignationName = model.DesignationName;
                    Designation.Status = model.Status;
                }
                else
                    Designation = model;

                if (Designation.DesignationId == 0)
                    dbContext.Designations.Add(Designation);
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
        [Route("deleteDesignation")]
        public ExpandoObject DeleteDesignation(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Designation model = JsonConvert.DeserializeObject<Designation>(decryptData);

                var Designation = dbContext.Designations.Where(x => x.DesignationId == model.DesignationId).First();
                dbContext.Designations.Remove(Designation);
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
