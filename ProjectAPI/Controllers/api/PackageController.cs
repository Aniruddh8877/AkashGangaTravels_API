using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Package")]
    public class PackageController : ApiController
    {
        [HttpPost]
        [Route("PackageList")]
        public ExpandoObject PackeageList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string appKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Package model = JsonConvert.DeserializeObject<Package>(decryptData);

                var list = dbContext.Packages
      .Where(p => model.DestinationId == 0 || p.DestinationId == model.DestinationId)
      .Select(s => new
      {
          s.PackageId,
          s.PackageCode,
          s.PackageName,
          s.Description,
          s.Destination.DestinationName,
          s.Status,
          s.CreatedBy,
          s.CreatedOn,
          s.UpdatedBy,
          s.UpdatedOn,
      })
      .ToList();



                res.PackageList = list;
                res.Message = ConstantData.SuccessMessage;

            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost]
        [Route("ListByDestination")]
        public ExpandoObject GetPackagesByDestination(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    var model = JsonConvert.DeserializeObject<Package>(decryptData);

                    var list = dbContext.Packages
                        .Where(p => model.DestinationId == 0 || p.DestinationId == model.DestinationId)
                        .Select(s => new
                        {
                            s.PackageId,
                            s.PackageCode,
                            s.PackageName,
                            s.Description,
                            s.Destination.DestinationName,
                            s.Status,
                            s.CreatedBy,
                            s.CreatedOn,
                            s.UpdatedBy,
                            s.UpdatedOn,
                        }).ToList();

                    res.PackageList = list;
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
        [Route("savePackage")]
        public ExpandoObject savePackage(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Package model = JsonConvert.DeserializeObject<Package>(decryptData);

                    Package Package;

                    if (model.PackageId > 0)
                    {
                        Package = dbContext.Packages.FirstOrDefault(x => x.PackageId == model.PackageId);
                        if (Package == null)
                        {
                            response.Message = "Destination not found.";
                        }
                        Package.PackageName = model.PackageName;
                        Package.DestinationId = model.DestinationId;
                        Package.Description = model.Description;
                        Package.Status = model.Status;
                    }
                    else
                    {
                        Package = model;
                        Package.PackageCode = AppData.GeneratePackageCode(dbContext);
                        Package.CreatedOn = DateTime.Now;
                        dbContext.Packages.Add(Package);
                    }

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

        [HttpPost]
        [Route("deletePackage")]
        public ExpandoObject deletePackage(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Package model = JsonConvert.DeserializeObject<Package>(decryptData);
                    var package = dbContext.Packages.FirstOrDefault(x => x.PackageId == model.PackageId);
                    if (package == null)
                    {
                        response.Message = "hotelCategory not found.";
                    }
                    dbContext.Packages.Remove(package);
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
