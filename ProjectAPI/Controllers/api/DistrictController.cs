using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/District")]
    public class DistrictController : ApiController
    {
        [HttpPost]
        [Route("DistrictList")]
        public ExpandoObject DistrictList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                District model = JsonConvert.DeserializeObject<District>(decryptData);

                var list = (from d1 in dbContext.Districts
                            where (model.DistrictId == d1.DistrictId || model.DistrictId == 0)
                            && (model.StateId == d1.StateId || model.StateId == 0)
                            orderby d1.DistrictName
                            select new
                            {
                                d1.DistrictId,
                                d1.DistrictName,
                                d1.StateId,
                                d1.State.StateName,
                                d1.State.StateCode
                            }).ToList();

                response.DistrictList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveDistrict")]
        public ExpandoObject SaveDistrict(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                District model = JsonConvert.DeserializeObject<District>(decryptData);

                District District = null;
                if (model.DistrictId > 0)
                {
                    District = dbContext.Districts.Where(x => x.DistrictId == model.DistrictId).First();
                    District.DistrictName = model.DistrictName;
                    District.StateId = model.StateId;
                }
                else
                    District = model;

                if (District.DistrictId == 0)
                    dbContext.Districts.Add(District);
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
        [Route("deleteDistrict")]
        public ExpandoObject DeleteDistrict(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                District model = JsonConvert.DeserializeObject<District>(decryptData);

                var District = dbContext.Districts.Where(x => x.DistrictId == model.DistrictId).First();
                dbContext.Districts.Remove(District);
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
