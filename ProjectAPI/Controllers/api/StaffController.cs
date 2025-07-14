using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Staff")]
    public class StaffController : ApiController
    {
        [HttpPost]
        [Route("StaffList")]
        public ExpandoObject StaffList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Staff model = JsonConvert.DeserializeObject<Staff>(decryptData);
                var list = (from d1 in dbContext.Staffs
                            join d2 in dbContext.Designations on d1.DesignationId equals d2.DesignationId
                            join d3 in dbContext.Departments on d1.DepartmentId equals d3.DepartmentId
                            where (model.StaffId == d1.StaffId || model.StaffId == 0) && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.StaffName
                            select new
                            {
                                d1.StaffId,
                                d1.StaffName,
                                d1.StaffCode,
                                d1.Status,
                                d1.DesignationId,
                                d2.DesignationName,
                                d1.StaffType,
                                d1.FatherName,
                                d1.MobileNo,
                                d1.AlternateNo,
                                d1.Email,
                                d1.Gender,
                                d1.StaffPhoto,
                                d1.Qualification,
                                d1.FullAddress,
                                d1.JoinDate,
                                d1.DepartmentId,
                                d3.DepartmentName,
                                d1.UpdatedBy,
                                d1.UpdatedDate,
                                d1.CreatedBy,
                                d1.CreatedDate,
                                d1.DOB,
                            }).ToList();

                response.StaffList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveStaff")]
        public ExpandoObject SaveStaff(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Staff model = JsonConvert.DeserializeObject<Staff>(decryptData);

                Staff Staff = null;
                if (model.StaffId > 0)
                {
                    Staff = dbContext.Staffs.Where(x => x.StaffId == model.StaffId).First();
                    Staff.UpdatedBy = model.UpdatedBy;
                    Staff.UpdatedDate = DateTime.Now;
                    Staff.StaffName = model.StaffName;
                    Staff.Status = model.Status;
                    Staff.DesignationId = model.DesignationId;
                    Staff.StaffType = model.StaffType;
                    Staff.FatherName = model.FatherName;
                    Staff.MobileNo = model.MobileNo;
                    Staff.AlternateNo = model.AlternateNo;
                    Staff.Email = model.Email;
                    Staff.Gender = model.Gender;
                    Staff.StaffPhoto = model.StaffPhoto;
                    Staff.Qualification = model.Qualification;
                    Staff.FullAddress = model.FullAddress;
                    Staff.JoinDate = model.JoinDate;
                    Staff.DepartmentId = model.DepartmentId;
                    Staff.DOB = model.DOB;
                }
                else
                {
                    Staff = model;
                    Staff.CreatedDate = DateTime.Now;
                    Staff.StaffCode = AppData.GenerateStaffCode(dbContext);
                }

                if (Staff.StaffId == 0)
                    dbContext.Staffs.Add(Staff);
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
        [Route("deleteStaff")]
        public ExpandoObject DeleteStaff(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Staff model = JsonConvert.DeserializeObject<Staff>(decryptData);
                var Staff = dbContext.Staffs.Where(x => x.StaffId == model.StaffId).First();
                dbContext.Staffs.Remove(Staff);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                    response.Message = "This record is in use. so can't delete.";
                else
                    response.Message = ex.Message;
            }
            return response;
        }
    }
}
