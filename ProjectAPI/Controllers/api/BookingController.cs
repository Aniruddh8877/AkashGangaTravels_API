using Newtonsoft.Json;
using Project;
using System;
using System.Data.Entity.Validation;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Booking")]
    public class BookingController : ApiController
    {
        public class BookingFilterModel
        {
            public int DestinationId { get; set; }
            public int Months { get; set; }
            public int BookingStatus { get; set; }

            public int AgentId { get; set; }
        }


        [HttpPost]
        [Route("SaveBooking")]
        public ExpandoObject SaveBooking(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();

            using (AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    BookingModel model = JsonConvert.DeserializeObject<BookingModel>(decryptData);

                    Booking booking;
                    int BookingId;

                    // 📝 Insert or Update Booking
                    if (model.GetBooking.BookingId > 0)
                    {
                        booking = dbContext.Bookings.FirstOrDefault(x => x.BookingId == model.GetBooking.BookingId);
                        if (booking != null)
                        {
                            booking.BookingDate = model.GetBooking.BookingDate;
                            booking.EnquiryId = model.GetBooking.EnquiryId;
                            booking.AgentId = model.GetBooking.AgentId;
                            booking.FlightOption = model.GetBooking.FlightOption;
                            booking.DestinationId = model.GetBooking.DestinationId;
                            booking.PackageId = model.GetBooking.PackageId;
                            booking.ArivalDate = model.GetBooking.ArivalDate;
                            booking.NoOfDay = model.GetBooking.NoOfDay;
                            booking.DepartureDate = model.GetBooking.DepartureDate;
                            booking.HotelCategoryId = model.GetBooking.HotelCategoryId;
                            booking.NoOfPerson = model.GetBooking.NoOfPerson;
                            booking.Rate = model.GetBooking.Rate;
                            booking.NoOfRoom = model.GetBooking.NoOfRoom;
                            booking.TotalAmount = model.GetBooking.TotalAmount;
                            booking.BookingStatus = model.GetBooking.BookingStatus;
                            booking.MealPlan = model.GetBooking.MealPlan;
                            booking.UpdatedBy = model.GetBooking.UpdatedBy;
                            booking.UpdatedOn = DateTime.Now;
                        }

                        BookingId = model.GetBooking.BookingId;
                    }
                    else
                    {
                        booking = new Booking
                        {
                            BookingDate = DateTime.Now,
                            EnquiryId = model.GetBooking.EnquiryId,
                            AgentId = model.GetBooking.AgentId,
                            FlightOption = model.GetBooking.FlightOption,
                            BookingCode = AppData.GenerateBookingCode(dbContext),
                            DestinationId = model.GetBooking.DestinationId,
                            HotelCategoryId = model.GetBooking.HotelCategoryId,
                            PackageId = model.GetBooking.PackageId,
                            ArivalDate = model.GetBooking.ArivalDate
                            ,
                            NoOfDay = model.GetBooking.NoOfDay,
                            DepartureDate = model.GetBooking.DepartureDate,
                            NoOfPerson = model.GetBooking.NoOfPerson,
                            NoOfRoom = model.GetBooking.NoOfRoom,
                            Rate = model.GetBooking.Rate,
                            TotalAmount = model.GetBooking.TotalAmount,
                            BookingStatus = (int)BookingStatus.TourPending,
                            CreatedBy = model.GetBooking.CreatedBy,
                            MealPlan = model.GetBooking.MealPlan,
                            CreatedOn = DateTime.Now
                        };

                        dbContext.Bookings.Add(booking);
                        dbContext.SaveChanges();
                        BookingId = booking.BookingId;
                    }


                    // 👥 Handle Booking Guests (both primary and non-primary)
                    if (model.GetGuests != null && model.GetGuests.Any())
                    {
                        var existing = dbContext.Guests.Where(x => x.BookingId == BookingId && x.isPrimary == false).ToList();
                        var incomingIds = model.GetGuests.Where(x => x.GuestId > 0).Select(x => x.GuestId).ToList();
                        var toDelete = existing.Where(x => !incomingIds.Contains(x.GuestId)).ToList();
                        dbContext.Guests.RemoveRange(toDelete);

                        foreach (var guest in model.GetGuests)
                        {
                            if (guest.GuestId > 0)
                            {
                                var existingGuest = dbContext.Guests.FirstOrDefault(x => x.GuestId == guest.GuestId);
                                if (existingGuest != null)
                                {
                                    existingGuest.Title = guest.Title;
                                    existingGuest.GuestName = guest.GuestName;
                                    existingGuest.MobileNo = guest.MobileNo;
                                    existingGuest.Age = guest.Age;
                                    existingGuest.DOB = guest.DOB;
                                    existingGuest.IDTypeId = guest.IDTypeId;
                                    existingGuest.IDNo = guest.IDNo;
                                    existingGuest.GSTNo = guest.GSTNo;
                                    existingGuest.isPrimary = guest.isPrimary;
                                }
                            }
                            else
                            {
                                // ✅ Check duplication for primary guest
                                bool isPrimary = guest.isPrimary == true;
                                bool exists = dbContext.Guests.Any(x =>
                                    x.MobileNo == guest.MobileNo &&
                                    x.GuestName == guest.GuestName &&
                                    x.isPrimary == guest.isPrimary);

                                // ❗ Insert only if:
                                // - not primary OR
                                // - is primary and doesn't already exist
                                if (!isPrimary || (isPrimary && !exists))
                                {
                                    var newGuest = new Guest
                                    {
                                        BookingId = BookingId,
                                        Title = guest.Title,
                                        GuestName = guest.GuestName,
                                        MobileNo = guest.MobileNo,
                                        Age = guest.Age,
                                        DOB = guest.DOB,
                                        IDTypeId = guest.IDTypeId,
                                        IDNo = guest.IDNo,
                                        GSTNo = guest.GSTNo,
                                        isPrimary = guest.isPrimary
                                    };

                                    dbContext.Guests.Add(newGuest);
                                }
                            }
                        }

                        dbContext.SaveChanges();
                    }
                    //Updating Enquiry Status 
                    var Enquiry = dbContext.Enquiries.First(x => x.EnquiryId == booking.EnquiryId);
                    Enquiry.EnquiryStatus = (byte)EnquiryStatus.Confirm;
                    dbContext.SaveChanges();

                    transaction.Commit();
                    response.Message = ConstantData.SuccessMessage;
                    response.BookingId = BookingId;
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();
                    response.Message = string.Join("; ", ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => $"Property: {x.PropertyName}, Error: {x.ErrorMessage}"));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    response.Message = ex.Message;
                }
            }

            return response;
        }


        [HttpPost]
        [Route("BookingList")]
        public ExpandoObject BookingList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, appKey, (byte)KeyFor.Admin);

                    // 🔐 Decrypt incoming data
                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    BookingFilterModel model = JsonConvert.DeserializeObject<BookingFilterModel>(decryptData);

                    // ✅ Apply filters including nullable CreatedOn
                    var list = db.Bookings
                        .Where(a =>
                            (model.DestinationId == 0 || a.DestinationId == model.DestinationId) &&
                            (model.Months == 0 || (a.CreatedOn.HasValue && a.CreatedOn.Value.Month == model.Months)) &&
                            (model.BookingStatus == 0 || a.BookingStatus == model.BookingStatus) &&
                            (model.AgentId == 0 || a.AgentId == model.AgentId)
                        )
                        .Select(a => new
                        {
                            a.BookingId,
                            a.BookingCode,
                            a.BookingDate,
                            a.EnquiryId,
                            EnquiryCode = a.Enquiry != null ? a.Enquiry.EnquiryCode : "",
                            a.AgentId,
                            AgentName = a.Agent != null ? a.Agent.ContactPersonName : "",
                            a.FlightOption,
                            a.DestinationId,
                            DestinationName = a.Destination != null ? a.Destination.DestinationName : "",
                            a.PackageId,
                            PackageName = a.Package != null ? a.Package.PackageName : "",
                            a.HotelCategoryId,
                            HotelCategoryName = a.HotelCategory.HotelCategoryName,
                            a.NoOfDay,
                            TravelPlanDate = a.ArivalDate,
                            a.DepartureDate,
                            a.NoOfPerson,
                            AmountQuoted = a.Rate,
                            a.NoOfRoom,
                            a.TotalAmount,
                            a.BookingStatus,
                            a.CreatedBy,
                            StaffName = a.StaffLogin != null && a.StaffLogin.Staff != null ? a.StaffLogin.Staff.StaffName : "",
                            a.CreatedOn,
                            a.MealPlan,
                            a.UpdatedBy,
                            a.UpdatedOn,
                        })
                        .OrderByDescending(x => x.BookingId)
                        .ToList();

                    res.BookingList = list;
                    res.Message = ConstantData.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
            return res;
        }

        public class ArrivalBookingFilterModel
        {
            public int Months { get; set; }
        }
        [HttpPost]
        [Route("ArrivalBookingList")]
        public ExpandoObject ArrivalBookingList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    ArrivalBookingFilterModel model = JsonConvert.DeserializeObject<ArrivalBookingFilterModel>(decryptData);

                    DateTime today = DateTime.Today; // ✅ Today’s date (ignores time)

                    int currentMonth = DateTime.Today.Month;

                    var list = db.Bookings
                        .Where(a =>
                            (a.ArivalDate != null && a.ArivalDate.Month == currentMonth) &&
                            (model.Months == 0 || (a.CreatedOn.HasValue && a.CreatedOn.Value.Month == model.Months))
                        ).Select(a => new
                     {
                         a.ArivalDate,
                         a.BookingId,
                         a.BookingCode,
                         a.BookingDate,
                         a.EnquiryId,
                         EnquiryCode = a.Enquiry != null ? a.Enquiry.EnquiryCode : "",
                         a.AgentId,
                         AgentName = a.Agent != null ? a.Agent.ContactPersonName : "",
                         a.FlightOption,
                         a.DestinationId,
                         DestinationName = a.Destination != null ? a.Destination.DestinationName : "",
                         a.PackageId,
                         PackageName = a.Package != null ? a.Package.PackageName : "",
                         a.HotelCategoryId,
                         HotelCategoryName = a.HotelCategory.HotelCategoryName,
                         a.NoOfDay,
                         TravelPlanDate = a.ArivalDate,
                         a.DepartureDate,
                         a.NoOfPerson,
                         AmountQuoted = a.Rate,
                         a.NoOfRoom,
                         a.TotalAmount,
                         a.BookingStatus,
                         a.CreatedBy,
                         StaffName = a.StaffLogin != null && a.StaffLogin.Staff != null
                      ? a.StaffLogin.Staff.StaffName : "",
                         a.CreatedOn,
                         a.MealPlan,
                         a.UpdatedBy,
                         a.UpdatedOn,
                     })
     .OrderBy(x => x.ArivalDate)   // ✅ Sorted by latest arrival date
     .ToList();


                    res.ArrivalBookingList = list;
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
        [Route("EditBookingList")]
        public ExpandoObject EditBookingList(RequestModel requestModel)
        {
            dynamic res = new ExpandoObject();
            try
            {
                using (var db = new AkashGangaTravelEntities())
                {
                    string appKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(db, appKey, (byte)KeyFor.Admin);

                    string decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    Booking model = JsonConvert.DeserializeObject<Booking>(decryptData);

                    var list = db.Bookings.Where(b => b.BookingId == model.BookingId).Select(a => new
                    {
                        a.BookingId,
                        a.BookingCode,
                        a.BookingDate,
                        a.EnquiryId,
                        EnquiryCode = a.Enquiry != null ? a.Enquiry.EnquiryCode : "",
                        a.Enquiry.PrimaryGuestName,
                        a.Enquiry.MobileNo,
                        GuestName = a.Enquiry.PrimaryGuestName,
                        a.AgentId,
                        AgentName = a.Agent != null ? a.Agent.ContactPersonName : "",
                        a.FlightOption,
                        a.DestinationId,
                        DestinationName = a.Destination != null ? a.Destination.DestinationName : "",
                        a.PackageId,
                        PackageName = a.Package != null ? a.Package.PackageName : "",
                        a.HotelCategoryId,
                        HotelCategoryName = a.HotelCategory.HotelCategoryName,
                        a.NoOfDay,
                        TravelPlanDate = a.ArivalDate,
                        a.DepartureDate,
                        a.NoOfPerson,
                        AmountQuoted = a.Rate,
                        a.NoOfRoom,
                        a.TotalAmount,
                        a.BookingStatus,
                        a.CreatedBy,
                        StaffName = a.StaffLogin != null && a.StaffLogin.Staff != null ? a.StaffLogin.Staff.StaffName : "",
                        a.CreatedOn,
                        a.MealPlan,
                        a.UpdatedBy,
                        a.UpdatedOn,
                        SearchEnquiry = a.Enquiry != null ? a.Enquiry.EnquiryCode + "-" + a.Enquiry.PrimaryGuestName + "-" + a.Enquiry.MobileNo : ""
                    }).OrderByDescending(x => x.BookingId).First();

                    res.EditBookingList = list;
                    var listGuest = db.Guests.Where(g => g.BookingId == model.BookingId).Select(c => new
                    {
                        c.GuestId,
                        c.GuestName,
                        c.Title,
                        c.Age,
                        c.MobileNo,
                        c.DOB,
                        c.IDTypeId,
                        c.IDNo,
                        c.GSTNo,

                    }).OrderByDescending(x => x.GuestId).ToList();

                    res.SelectedGuestDetailList = listGuest;
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
