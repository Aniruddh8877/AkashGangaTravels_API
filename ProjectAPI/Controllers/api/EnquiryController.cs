using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
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
            //public int EnquiryStatus { get; set; }
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
                    .Where(a =>
                        (model.DestinationId == 0 || a.DestinationId == model.DestinationId) &&
                        (model.Months == 0 || a.CreatedOn.Month == model.Months) &&
                        (!model.FromDate.HasValue || a.CreatedOn >= model.FromDate.Value) &&
                        (!model.ToDate.HasValue || a.CreatedOn <= model.ToDate.Value)
                        //(model.EnquiryStatus == 0 || a.EnquiryStatus == model.EnquiryStatus)
                    )
                    .Select(a => new
                    {
                        a.EnquiryId,
                        a.EnquiryStatus,
                        a.AmountQuoted,
                        a.CreatedBy,
                        a.CreatedOn,
                        a.DestinationId,
                        DestinationName = a.Destination.DestinationName,
                        a.EnquiryCode,
                        a.FlightOption,
                        a.HotelCategoryId,
                        HotelCategoryName = a.HotelCategory.HotelCategoryName,
                        a.MealPlan,
                        a.MobileNo,
                        a.NoOfPerson,
                        a.NoOfRoom,
                        a.PackageId,
                        PackageName = a.Package.PackageName,
                        a.PrimaryGuestName,
                        a.Remarks,
                        a.TravelPlanDate,
                        StaffName = a.StaffLogin.Staff.StaffName,
                        a.Title,
                        //a.IsPrimaryGuest,
                    })
                    .ToList();

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
        [Route("EnquiryListById")]
        public ExpandoObject EnquiryListById(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string appKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Enquiry model = JsonConvert.DeserializeObject<Enquiry>(decryptData);

                var enquiry = dbContext.Enquiries
                    .Where(a => a.EnquiryId == model.EnquiryId)
                    .Select(a => new
                    {
                        a.EnquiryId,
                        a.EnquiryStatus,
                        a.AmountQuoted,
                        a.CreatedBy,
                        a.CreatedOn,
                        a.DestinationId,
                        DestinationName = a.Destination.DestinationName,
                        a.EnquiryCode,
                        a.FlightOption,
                        a.HotelCategoryId,
                        HotelCategoryName = a.HotelCategory.HotelCategoryName,
                        a.MealPlan,
                        a.MobileNo,
                        a.NoOfPerson,
                        a.NoOfRoom,
                        a.PackageId,
                        PackageName = a.Package.PackageName,
                        a.PrimaryGuestName,
                        a.Remarks,
                        a.TravelPlanDate,
                        StaffName = a.StaffLogin.Staff.StaffName,
                        a.Title,
                        //a.IsPrimaryGuest,
                    })
                    .FirstOrDefault();

                if (enquiry != null)
                {
                    res.EnquiryDetails = enquiry;
                    res.Message = ConstantData.SuccessMessage;
                }
                else
                {
                    res.Message = "Enquiry not found.";
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }

            return res;
        }


        [HttpPost]
        [Route("SaveEnquiry")]
        public ExpandoObject SaveEnquiry(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();

            using (AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    EnquiryGuestModel model = JsonConvert.DeserializeObject<EnquiryGuestModel>(decryptData);

                    // Step 1: Check if guest already exists (based on MobileNo + GuestName)
                    Guest guest = dbContext.Guests.FirstOrDefault(x =>
                            x.MobileNo == model.GetGuests.MobileNo &&
                            x.GuestName == model.GetGuests.GuestName);
                    if (guest == null)
                    {
                        if (model.GetEnquiry.GuestId == 0)
                        {
                            model.GetGuests.GuestName = model.GetGuests.GuestName;
                            model.GetGuests.MobileNo = model.GetGuests.MobileNo;
                            model.GetGuests.Title = model.GetGuests.Title;
                            model.GetGuests.isPrimary = true;
                            dbContext.Guests.Add(model.GetGuests);
                            dbContext.SaveChanges();
                            model.GetEnquiry.GuestId = model.GetGuests.GuestId;
                            model.GetEnquiry.PrimaryGuestName = model.GetGuests.GuestName;
                            model.GetEnquiry.MobileNo = model.GetGuests.MobileNo;
                            model.GetEnquiry.Title = model.GetGuests.Title;
                        }
                        else
                        {
                            guest = dbContext.Guests.Where(x => x.GuestId == model.GetEnquiry.GuestId).First();
                            guest.GuestName = model.GetGuests.GuestName;
                            guest.MobileNo = model.GetGuests.MobileNo;
                            guest.Title = model.GetGuests.Title;
                            dbContext.SaveChanges();
                        }
                    }

                    //if (guest == null)
                    //{
                    //    guest = new Guest
                    //    {
                    //        GuestName = model.GetEnquiry.PrimaryGuestName,
                    //        MobileNo = model.GetEnquiry.MobileNo,
                    //        Title = model.GetEnquiry.Title,
                    //        isPrimary = true
                    //    };
                    //    dbContext.Guests.Add(guest);
                    //    dbContext.SaveChanges(); // to get GuestId
                    //}

                    // Step 2: Insert or Update Enquiry
                    Enquiry enquiry;
                    if (model.GetEnquiry.EnquiryId > 0)
                    {
                        enquiry = dbContext.Enquiries.FirstOrDefault(x => x.EnquiryId == model.GetEnquiry.EnquiryId);
                        if (enquiry == null)
                        {
                            res.Message = "Enquiry not found.";
                            return res;
                        }

                        enquiry.DestinationId = model.GetEnquiry.DestinationId;
                        enquiry.PackageId = model.GetEnquiry.PackageId;
                        enquiry.TravelPlanDate = model.GetEnquiry.TravelPlanDate;
                        enquiry.FlightOption = model.GetEnquiry.FlightOption;
                        enquiry.NoOfRoom = model.GetEnquiry.NoOfRoom;
                        enquiry.MealPlan = model.GetEnquiry.MealPlan;
                        enquiry.HotelCategoryId = model.GetEnquiry.HotelCategoryId;
                        enquiry.Title = model.GetEnquiry.Title;
                        enquiry.MobileNo = model.GetEnquiry.MobileNo;
                        enquiry.PrimaryGuestName = model.GetEnquiry.PrimaryGuestName;
                        enquiry.NoOfPerson = model.GetEnquiry.NoOfPerson;
                        enquiry.EnquiryStatus = model.GetEnquiry.EnquiryStatus;
                        enquiry.AmountQuoted = model.GetEnquiry.AmountQuoted;
                        enquiry.Remarks = model.GetEnquiry.Remarks;
                        enquiry.UpdatedBy = model.GetEnquiry.UpdatedBy;
                        enquiry.UpdatedOn = DateTime.Now;
                        enquiry.GuestId = model.GetEnquiry.GuestId; // updated guest ref
                    }
                    else
                    {
                        enquiry = new Enquiry
                        {
                            DestinationId = model.GetEnquiry.DestinationId,
                            PackageId = model.GetEnquiry.PackageId,
                            TravelPlanDate = model.GetEnquiry.TravelPlanDate,
                            FlightOption = model.GetEnquiry.FlightOption,
                            NoOfRoom = model.GetEnquiry.NoOfRoom,
                            MealPlan = model.GetEnquiry.MealPlan,
                            HotelCategoryId = model.GetEnquiry.HotelCategoryId,
                            Title = model.GetEnquiry.Title,
                            MobileNo = model.GetEnquiry.MobileNo,
                            PrimaryGuestName = model.GetEnquiry.PrimaryGuestName,
                            NoOfPerson = model.GetEnquiry.NoOfPerson,
                            EnquiryStatus = (int)EnquiryStatus.Pending,
                            AmountQuoted = model.GetEnquiry.AmountQuoted,
                            Remarks = model.GetEnquiry.Remarks,
                            EnquiryCode = AppData.GenerateEnquiryCode(dbContext),
                            CreatedBy = model.GetEnquiry.CreatedBy,
                            CreatedOn = DateTime.Now,
                            GuestId = model.GetEnquiry.GuestId
                        };
                        dbContext.Enquiries.Add(enquiry);
                    }

                    dbContext.SaveChanges();
                    transaction.Commit();

                    res.Message = ConstantData.SuccessMessage;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    res.Message = "Error: " + ex.Message;
                }
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