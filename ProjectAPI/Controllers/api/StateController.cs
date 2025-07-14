using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/State")]
    public class StateController : ApiController
    {
        [HttpPost]
        [Route("StateList")]
        public ExpandoObject StateList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                State model = JsonConvert.DeserializeObject<State>(decryptData);

                var list = (from d1 in dbContext.States
                           where (model.StateId == d1.StateId || model.StateId == 0)
                           && (model.Status == d1.Status || model.Status == 0)
                           orderby d1.StateName
                            select new
                            {
                                d1.StateId,
                                d1.StateName,
                                d1.StateCode,
                                d1.Status,
                            }).ToList();

                response.StateList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveState")]
        public ExpandoObject SaveState(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                State model = JsonConvert.DeserializeObject<State>(decryptData);

                State State = null;
                if (model.StateId > 0)
                {
                    State = dbContext.States.Where(x => x.StateId == model.StateId).First();
                    State.UpdatedBy = model.UpdatedBy;
                    State.StateName = model.StateName;
                    State.StateCode = model.StateCode;
                    State.Status = model.Status;
                }
                else
                    State = model;
                State.UpdatedDate = model.UpdatedDate;

                if (State.StateId == 0)
                    dbContext.States.Add(State);
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
        [Route("deleteState")]
        public ExpandoObject DeleteState(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                State model = JsonConvert.DeserializeObject<State>(decryptData);

                var State = dbContext.States.Where(x => x.StateId == model.StateId).First();
                dbContext.States.Remove(State);
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
