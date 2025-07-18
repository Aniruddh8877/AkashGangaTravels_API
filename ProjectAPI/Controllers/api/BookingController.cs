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
        [HttpPost]
        [Route("SaveResignation")]
        public ExpandoObject SaveResignation(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            using (AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities())
            {
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

                        if (model.GetBooking.BookingId > 0)
                        {
                            // Update existing LeaveRequest
                            booking = dbContext.Bookings.FirstOrDefault(x => x.BookingId == model.GetBooking.BookingId);
                            if ( booking!= null)
                            {
                                
                                booking.BookingDate = model.GetBooking.BookingDate;
                                booking.EnquiryId = model.GetBooking.EnquiryId;
                                booking.AgentId = model.GetBooking.AgentId;
                                booking.FlightOption = model.GetBooking.FlightOption;
                                //booking.BookingCode = model.GetBooking.BookingCode;
                                booking.DestinationId = model.GetBooking.DestinationId;
                                booking.PackageId = model.GetBooking.PackageId;
                                booking.ArivalDate = model.GetBooking.ArivalDate;
                                booking.NoOfDay = model.GetBooking.NoOfDay;
                                booking.DepartureDate = model.GetBooking.DepartureDate;
                                booking.NoOfPerson = model.GetBooking.NoOfPerson;
                                booking.Rate = model.GetBooking.Rate;
                                booking.NoOfRoom = model.GetBooking.NoOfRoom;
                                booking.TotalAmount = model.GetBooking.TotalAmount;
                                booking.BookingStatus = model.GetBooking.BookingStatus;
                                booking.UpdatedBy = model.GetBooking.UpdatedBy;
                                booking.UpdatedOn = DateTime.Now;
                            }
                            BookingId = model.GetBooking.BookingId;
                        }
                        else
                        {
                            // Insert new LeaveRequest
                            booking = new Booking
                            {
                                BookingDate = DateTime.Now,
                                EnquiryId = model.GetBooking.EnquiryId,
                                AgentId = model.GetBooking.AgentId,
                                FlightOption = model.GetBooking.FlightOption,
                                BookingCode = AppData.GenerateBookingCode(dbContext),
                                DestinationId = model.GetBooking.DestinationId,
                                PackageId = model.GetBooking.PackageId,
                                ArivalDate = model.GetBooking.ArivalDate,
                                NoOfDay = model.GetBooking.NoOfDay,
                                DepartureDate = model.GetBooking.DepartureDate,
                                NoOfPerson = model.GetBooking.NoOfPerson,
                                NoOfRoom = model.GetBooking.NoOfRoom,
                                Rate = model.GetBooking.Rate,
                                TotalAmount = model.GetBooking.TotalAmount,
                                BookingStatus =(int)BookingStatus.TourPending, // Example: Setting to Pending
                                CreatedBy = model.GetBooking.CreatedBy,      
                                CreatedOn = DateTime.Now                     
                            };                                               

                            dbContext.Bookings.Add(booking);
                            dbContext.SaveChanges();
                            BookingId = booking.BookingId;
                        }

                        if (model.GetGuests != null && model.GetGuests.Any())
                        {
                            // Fetch existing details for this LeadId
                            var existing = dbContext.Guests.Where(x => x.GuestId == BookingId).ToList();
                            var incomingIds = model.GetGuests.Where(x => x.GuestId > 0).Select(x => x.GuestId).ToList();

                            // Delete details not present in incoming list
                            var toDelete = existing.Where(x => !incomingIds.Contains(x.GuestId)).ToList();
                            dbContext.Guests.RemoveRange(toDelete);

                            foreach (var detail in model.GetGuests)
                            {
                                if (detail.GuestId > 0)
                                {
                                    // Update existing
                                    var existingDetail = dbContext.Guests.FirstOrDefault(x => x.GuestId == detail.GuestId);
                                    if (existingDetail != null)
                                    {

                                        existingDetail.GuestName = detail.GuestName;
                                        existingDetail.Title = detail.Title;
                                        existingDetail.Age = detail.Age;
                                        existingDetail.MobileNo = detail.MobileNo;
                                        existingDetail.DOB = detail.DOB;
                                        existingDetail.IDTypeId = detail.IDTypeId;
                                        existingDetail.IDNo = detail.IDNo;
                                        existingDetail.GSTNo = detail.GSTNo;
                                       
                                    }
                                }
                                else
                                {
                                    // Insert new
                                    var newDetail = new Guest
                                    {
                                        BookingId = BookingId,
                                        GuestName = detail.GuestName,
                                        MobileNo = detail.MobileNo,
                                        Age = detail.Age,
                                        DOB = detail.DOB,
                                        IDTypeId = detail.IDTypeId,
                                        IDNo = detail.IDNo,
                                        GSTNo= detail.GSTNo,
                                        Title = detail.Title,
                                       
                                    };
                                    dbContext.Guests.Add(newDetail);
                                }
                            }

                            dbContext.SaveChanges();
                        }


                        dbContext.SaveChanges();
                        transaction.Commit();

                        response.Message = ConstantData.SuccessMessage;
                        response.BookingId = BookingId;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        transaction.Rollback();
                        var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => $"Property: {x.PropertyName}, Error: {x.ErrorMessage}");
                        string fullError = string.Join("; ", errorMessages);
                        response.Message = fullError;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.Message = ex.Message;
                    }
                }
            }
            return response;
        }
    }
}
