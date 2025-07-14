using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/HotelCategory")]
    public class HotelCategoryController : ApiController
    {
        [HttpPost]
        [Route("HotelCategoryList")]
        public ExpandoObject HotelCategoryList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string appKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                HotelCategory model = JsonConvert.DeserializeObject<HotelCategory>(decryptData);

                var list = dbContext.HotelCategories.Select(s => new
                {
                    s.HotelCategoryId,
                    s.HotelCategoryName,
                    s.Status,
                }).ToList();

                res.HotelCategoryList = list;
                res.Message = ConstantData.SuccessMessage;
            }
            catch(Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost]
        [Route("saveHotelCateogry")]
        public ExpandoObject saveHotelCateogry(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    HotelCategory model = JsonConvert.DeserializeObject<HotelCategory>(decryptData);

                    HotelCategory hotelCategory;

                    if (model.HotelCategoryId > 0)
                    {
                        hotelCategory = dbContext.HotelCategories.FirstOrDefault(x => x.HotelCategoryId == model.HotelCategoryId);
                        if (hotelCategory == null)
                        {
                            response.Message = "Destination not found.";
                        }
                        hotelCategory.HotelCategoryName = model.HotelCategoryName;
                        hotelCategory.Status = model.Status;
                    }
                    else
                    {
                        hotelCategory = model;
                        dbContext.HotelCategories.Add(hotelCategory);
                    }

                    dbContext.SaveChanges();
                    response.Message = ConstantData.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                // Optional: Handle specific unique constraint error
                if (ex.InnerException?.InnerException?.Message.Contains("IX_HotelCategory") == true)
                {
                    response.Message = "Hotel category already exists.";
                }
                else
                {
                    response.Message = "An error occurred while saving.";
                }
            }
            return response;
        }

        [HttpPost]
        [Route("deleteHotelCategory")]
        public ExpandoObject deleteHotelCategory(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    HotelCategory model = JsonConvert.DeserializeObject<HotelCategory>(decryptData);
                    var hotelCategory = dbContext.HotelCategories.FirstOrDefault(x => x.HotelCategoryId == model.HotelCategoryId);
                    if (hotelCategory == null)
                    {
                        response.Message = "hotelCategory not found.";
                    }
                    dbContext.HotelCategories.Remove(hotelCategory);
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
