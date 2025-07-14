using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/City")]
    public class CityController : ApiController
    {
        [HttpPost]
        [Route("CityList")]
        public ExpandoObject CityList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                City model = JsonConvert.DeserializeObject<City>(decryptData);

                var list = (from d1 in dbContext.Cities
                           join d2 in dbContext.States on d1.StateId equals d2.StateId
                           where (model.CityId == d1.CityId || model.CityId == 0)
                           && (model.StateId == d1.StateId || model.StateId == 0)
                           && (model.Status == d1.Status || model.Status == 0)
                           orderby d1.CityName
                           select new
                           {
                               d1.CityId,
                               d1.CityName,
                               d2.StateId,
                               d2.StateName,
                               d1.Status,
                           }).ToList();

                response.CityList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveCity")]
        public ExpandoObject SaveCity(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                City model = JsonConvert.DeserializeObject<City>(decryptData);

                City City = null;
                if (model.CityId > 0)
                {
                    City = dbContext.Cities.Where(x => x.CityId == model.CityId).First();
                    City.UpdatedBy = model.UpdatedBy;
                    City.CityName = model.CityName;
                    City.StateId = model.StateId;
                    City.Status = model.Status;
                }
                else
                    City = model;
                City.UpdatedDate = DateTime.Now;
                if (City.CityId == 0)
                    dbContext.Cities.Add(City);
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
        [Route("deleteCity")]
        public ExpandoObject DeleteCity(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                City model = JsonConvert.DeserializeObject<City>(decryptData);

                var City = dbContext.Cities.Where(x => x.CityId == model.CityId).First();
                dbContext.Cities.Remove(City);
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
