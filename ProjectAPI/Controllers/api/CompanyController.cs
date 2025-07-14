using Newtonsoft.Json;
using Project;
using ProjectAPI.Models;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Company")]
    public class CompanyController : ApiController
    {
        [HttpPost]
        [Route("CompanyList")]
        public ExpandoObject CompanyList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Company model = JsonConvert.DeserializeObject<Company>(decryptData);

                var list = (from s1 in dbContext.Companies
                            where (model.Status == 0 || s1.Status == model.Status)
                            && (model.CompanyId == 0 || s1.CompanyId == model.CompanyId)
                            orderby s1.CompanyId
                            select new
                            {
                                s1.CompanyId,
                                s1.CompanyName,
                                s1.ShortName,
                                s1.CompanyAddress,
                                s1.CityName,
                                s1.DistrictName,
                                s1.StateName,
                                s1.PinCode,
                                s1.Email,
                                s1.MobileNo,
                                s1.AlternateNo,
                                s1.Website,
                                s1.LogoPNG,
                                s1.Logo,
                                s1.Status,
                            }).ToList(); 
                response.CompanyList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveCompany")]
        public ExpandoObject SaveCompany(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Company model = JsonConvert.DeserializeObject<Company>(decryptData);
                Company Company = new Company();
                if (model.CompanyId > 0)
                {
                    Company = dbContext.Companies.Where(x => x.CompanyId == model.CompanyId).First();
                    Company.CompanyName = model.CompanyName;
                    Company.ShortName = model.ShortName;
                    Company.CompanyAddress = model.CompanyAddress;
                    Company.CityName = model.CityName;
                    Company.DistrictName = model.DistrictName;
                    Company.StateName = model.StateName;
                    Company.PinCode = model.PinCode;
                    Company.Email = model.Email;
                    Company.MobileNo = model.MobileNo;
                    Company.AlternateNo = model.AlternateNo;
                    Company.Website = model.Website;
                    Company.Status = model.Status;
                    Company.UpdatedBy = model.UpdatedBy;

                    if (!string.IsNullOrEmpty(model.LogoPNG) && model.LogoPNG != Company.LogoPNG)
                        Company.LogoPNG = Utils.SaveFile(model.LogoPNG, ConstantString.FileLocation, ".png");

                    if (!string.IsNullOrEmpty(model.Logo) && model.Logo != Company.Logo)
                        Company.Logo = Utils.SaveFile(model.Logo, ConstantString.FileLocation, ".jpg");
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.LogoPNG))
                        model.LogoPNG = Utils.SaveFile(model.LogoPNG, ConstantString.FileLocation, ".png");

                    if (!string.IsNullOrEmpty(model.Logo))
                        model.Logo = Utils.SaveFile(model.Logo, ConstantString.FileLocation, ".jpg");

                    Company = model;
                }
                Company.UpdatedOn = DateTime.Now;

               
                if (Company.CompanyId == 0)
                    dbContext.Companies.Add(Company);
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
        [Route("deleteCompany")]
        public ExpandoObject DeleteCompany(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);

                var Company = dbContext.Companies.Where(x => x.CompanyId == Convert.ToInt32(decryptData)).First();
                dbContext.Companies.Remove(Company);
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
