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
        public class HotelCategoryFilterModel
        {
            public int BookingId { get; set; }
        }

        [HttpPost]
        [Route("HotelCategoryList")]
        public ExpandoObject HotelCategoryList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    HotelCategory model = JsonConvert.DeserializeObject<HotelCategory>(decryptData);

                    var list = dbContext.HotelCategories
                        .Where(h => model.HotelCategoryId == 0 || h.HotelCategoryId == model.HotelCategoryId)
                        .Select(s => new
                        {
                            s.HotelCategoryId,
                            s.HotelCategoryName,
                            s.Status,
                        })
                        .ToList();

                    res.HotelCategoryList = list;
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
        [Route("HotelCategoryListByBooking")]
        public ExpandoObject HotelCategoryListByBooking(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    HotelCategoryFilterModel model = JsonConvert.DeserializeObject<HotelCategoryFilterModel>(decryptData);

                    var list = (from b in dbContext.Bookings
                                join h in dbContext.HotelCategories
                                on b.HotelCategoryId equals h.HotelCategoryId
                                where b.BookingId == model.BookingId
                                select new
                                {
                                    h.HotelCategoryId,
                                    h.HotelCategoryName,
                                    h.Status,
                                    b.NoOfRoom,
                                    b.FlightOption,
                                    b.MealPlan,
                                }).ToList();

                    res.HotelCategoryListByBooking = list;
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
                        hotelCategory = dbContext.HotelCategories
                            .FirstOrDefault(x => x.HotelCategoryId == model.HotelCategoryId);

                        if (hotelCategory == null)
                        {
                            response.Message = "Hotel category not found.";
                            return response;
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

                    var hotelCategory = dbContext.HotelCategories
                        .FirstOrDefault(x => x.HotelCategoryId == model.HotelCategoryId);

                    if (hotelCategory == null)
                    {
                        response.Message = "Hotel category not found.";
                        return response;
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
