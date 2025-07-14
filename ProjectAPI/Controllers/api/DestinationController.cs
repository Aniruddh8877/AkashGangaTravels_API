using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Destination")]
    public class DestinationController : ApiController
    {
        [HttpPost]
        [Route("DestinationList")]
        public ExpandoObject DestinationList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Destination model = JsonConvert.DeserializeObject<Destination>(decryptData);

                    var list = dbContext.Destinations.Select(s => new
                    {
                        s.DestinationId,
                        s.DestinationName,
                        s.DestinationType,
                        s.Status,
                        s.CreatedBy,
                        s.CreatedOn,
                        s.UpdatedBy,
                        s.UpdatedOn,
                    }).ToList();

                    res.DestinationList = list;
                    res.Message = ConstantData.SuccessMessage;
                   
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                
            }
            return res;
        }

        [HttpPost]
        [Route("saveDestination")]
        public ExpandoObject SaveDestination(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Destination model = JsonConvert.DeserializeObject<Destination>(decryptData);

                    Destination destination;

                    if (model.DestinationId > 0)
                    {
                        destination = dbContext.Destinations.FirstOrDefault(x => x.DestinationId == model.DestinationId);
                        if (destination == null)
                        {
                            response.Message = "Destination not found.";
                           
                        }

                        destination.DestinationName = model.DestinationName;
                        destination.DestinationType = model.DestinationType;
                        destination.Status = model.Status;
                        destination.UpdatedBy = model.UpdatedBy;
                        destination.UpdatedOn = DateTime.Now;
                    }
                    else
                    {
                        destination = model;
                        model.CreatedOn = DateTime.Now;
                        dbContext.Destinations.Add(destination);
                    }

                    dbContext.SaveChanges();
                    response.Message = ConstantData.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.InnerException?.Message.Contains("IX_Destination") == true)
                    response.Message = "A destination with this name already exists.";
                else
                    response.Message = "An error occurred while saving destination.";
            }

            return response;
        }

        [HttpPost]
        [Route("deleteDestination")]
        public ExpandoObject DeleteDestination(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Destination model = JsonConvert.DeserializeObject<Destination>(decryptData);

                    var destination = dbContext.Destinations.FirstOrDefault(x => x.DestinationId == model.DestinationId);
                    if (destination == null)
                    {
                        response.Message = "Destination not found.";
                        
                    }

                    dbContext.Destinations.Remove(destination);
                    dbContext.SaveChanges();

                    response.Message = ConstantData.SuccessMessage;
                  
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
               
            }
            return response;
        }
    }
}
