using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Enquiry")]
    public class EnquiryController : ApiController
    {
        public class EnquiryModel
        {
            public int DestinationId { get; set; }
            public int Months { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }

        }

        [HttpPost]
        [Route("EnquiryList")]
        public ExpandoObject EnquiryList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string appKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                EnquiryModel model = JsonConvert.DeserializeObject<EnquiryModel>(decryptData);


                var list = dbContext.Enquiries
     .Where(p =>
         (model.DestinationId == 0 || p.DestinationId == model.DestinationId) &&
         (model.Months == 0 || p.CreatedOn.Month == model.Months) &&
         (!model.FromDate.HasValue || p.CreatedOn >= model.FromDate.Value) &&
         (!model.ToDate.HasValue || p.CreatedOn <= model.ToDate.Value)
     )
     .Select(s => new
     {
         s.EnquiryId,
         StaffName = s.StaffLogin.Staff.StaffName,
         s.PackageId,
         s.DestinationId,
         s.HotelCategoryId,
         s.Destination.DestinationName,
         s.Package.PackageName,
         s.TravelPlanDate,
         s.FlightOption,
         s.NoOfPerson,
         s.NoOfRoom,
         s.MealPlan,
         s.HotelCategory.HotelCategoryName,
         s.PrimaryGuestName,
         s.MobileNo,
         s.AmountQuoted,
         s.Remarks,
         s.EnquiryStatus,
         s.EnquiryCode,
         s.CreatedBy,
         s.CreatedOn,
         s.UpdatedBy,
         s.UpdatedOn,
     }).ToList();


                res.EnquiryList = list;
                res.Message = ConstantData.SuccessMessage;

            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost]
        [Route("SaveEnquriy")]
        public ExpandoObject SaveEnquriy(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Enquiry model = JsonConvert.DeserializeObject<Enquiry>(decryptData);

                    Enquiry enquiry;

                    if (model.EnquiryId > 0)
                    {
                        enquiry = dbContext.Enquiries.FirstOrDefault(x => x.EnquiryId == model.EnquiryId);
                        if (enquiry == null)
                        {
                            res.Message = "Enquiry not found.";
                            return res;
                        }
                        enquiry.DestinationId = model.DestinationId;
                        enquiry.PackageId = model.PackageId;
                        enquiry.TravelPlanDate = model.TravelPlanDate;
                        enquiry.FlightOption = model.FlightOption;
                        enquiry.NoOfRoom = model.NoOfRoom;
                        enquiry.MealPlan = model.MealPlan;
                        enquiry.HotelCategoryId = model.HotelCategoryId;
                        enquiry.PrimaryGuestName = model.PrimaryGuestName;
                        enquiry.MobileNo = model.MobileNo;
                        enquiry.NoOfPerson = model.NoOfPerson;
                        enquiry.EnquiryStatus = model.EnquiryStatus;
                        enquiry.AmountQuoted = model.AmountQuoted;
                        enquiry.Remarks = model.Remarks;
                        enquiry.UpdatedBy = model.UpdatedBy;
                        enquiry.UpdatedOn = DateTime.Now;
                    }
                    else
                    {
                        enquiry = new Enquiry
                        {
                            DestinationId = model.DestinationId,
                            PackageId = model.PackageId,
                            TravelPlanDate = model.TravelPlanDate,
                            FlightOption = model.FlightOption,
                            NoOfRoom = model.NoOfRoom,
                            MealPlan = model.MealPlan,
                            HotelCategoryId = model.HotelCategoryId,
                            PrimaryGuestName = model.PrimaryGuestName,
                            MobileNo = model.MobileNo,
                            NoOfPerson = model.NoOfPerson,
                            EnquiryStatus = (int)EnquiryStatus.Pending,
                            AmountQuoted = model.AmountQuoted,
                            EnquiryCode = AppData.GenerateEnquiryCode(dbContext),
                            Remarks = model.Remarks,
                            CreatedBy = model.CreatedBy,
                            CreatedOn = DateTime.Now
                        };

                        dbContext.Enquiries.Add(enquiry);
                    }
                    dbContext.SaveChanges();
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
        [Route("deleteEnquriy")]
        public ExpandoObject deleteEnquriy(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                using (var dbContext = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);
                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Enquiry model = JsonConvert.DeserializeObject<Enquiry>(decryptData);
                    var enquiry = dbContext.Enquiries.FirstOrDefault(x => x.EnquiryId == model.EnquiryId);
                    if (enquiry == null)
                    {
                        response.Message = "hotelCategory not found.";
                    }
                    dbContext.Enquiries.Remove(enquiry);
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