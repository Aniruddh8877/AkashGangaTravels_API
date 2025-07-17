using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/IDType")]
    public class IDTypeController : ApiController
    {
        [HttpPost]
        [Route("IDTypeList")]
        public ExpandoObject IDTypeList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    // Validate AppKey
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, appKey, (byte)KeyFor.Admin);

                    //Optional decryption(not needed unless filters applied)
                     string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    IDType model = JsonConvert.DeserializeObject<IDType>(decryptData);

                    var list = db.IDTypes
                        .Select(a => new
                        {
                            a.IDTypeId,
                            a.IDTypeName,
                            a.Status,
                        }).ToList();

                    res.IDTypeList = list;
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
        [Route("saveIDType")]
        public ExpandoObject SaveIDType(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    IDType model = JsonConvert.DeserializeObject<IDType>(decryptData);

                    IDType iDType;
                    if (model.IDTypeId > 0)
                    {
                        iDType = db.IDTypes.FirstOrDefault(x => x.IDTypeId == model.IDTypeId);
                        if (iDType == null)
                        {
                            res.Message = "ID Type not found.";
                            res.Status = 404;
                            return res;
                        }
                        iDType.IDTypeName = model.IDTypeName;
                        iDType.Status = model.Status;
                    }
                    else
                    {
                        iDType = model;
                        db.IDTypes.Add(iDType);
                    }

                    db.SaveChanges();
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
        [Route("deleteIdType")]
        public ExpandoObject DeleteIDType(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    IDType model = JsonConvert.DeserializeObject<IDType>(decryptData);

                    var iDType = db.IDTypes.FirstOrDefault(x => x.IDTypeId == model.IDTypeId);
                    if (iDType == null)
                    {
                        res.Message = "ID Type not found.";
                        return res;
                    }

                    db.IDTypes.Remove(iDType);
                    db.SaveChanges();
                    res.Message = ConstantData.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }
    }
}
