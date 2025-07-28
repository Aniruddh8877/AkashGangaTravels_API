using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Guest")]
    public class GuestDetailController : ApiController
    {
        [HttpPost]
        [Route("GuestList")]
        public ExpandoObject GuestList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string appKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Guest model = JsonConvert.DeserializeObject<Guest>(decryptData);

                var list = dbContext.Guests
     .Where(a=>a.isPrimary==true)
     .Select(a => new
     {
         a.GuestId,
         a.GuestName,
         a.DOB,
         a.MobileNo,
         a.Age,
         a.Title,
         a.IDNo,
         a.IDType.IDTypeName,
         a.IDTypeId,
         a.GSTNo,
     })
     .ToList();

                res.GuestList = list;
                res.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }
    }
}
