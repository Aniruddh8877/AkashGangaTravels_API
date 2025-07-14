using Newtonsoft.Json;
using Project;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Data.Entity.Migrations;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/StaffLogin")]

    public class StaffLoginController : ApiController
    {
        [HttpPost]
        [Route("StaffLoginList")]
        public ExpandoObject StaffLoginList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StaffLogin model = JsonConvert.DeserializeObject<StaffLogin>(decryptData);

                var list = (from d1 in dbContext.StaffLogins
                            join d2 in dbContext.Staffs on d1.StaffId equals d2.StaffId
                            join s1 in dbContext.StaffLoginRoles on d1.StaffLoginId equals s1.StaffLoginId into subStaffLoginRole
                            where (model.StaffLoginId == d1.StaffLoginId || model.StaffLoginId == 0)
                            && (model.Status == d1.Status || model.Status == 0)
                            select new
                            {
                                d1.StaffLoginId,
                                d1.StaffId,
                                d2.StaffName,
                                d1.UserName,
                                d1.LoginPassword,
                                d1.Status,
                                StaffLoginRoleList = (from m1 in subStaffLoginRole
                                                     join m2 in dbContext.Roles on m1.RoleId equals m2.RoleId
                                                     select new { m1.RoleId, m1.StaffLoginRoleId, m2.RoleTitle }).ToList(),
                            }).ToList();

                response.StaffLoginList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public class SaveStaffLoginModel
        {
            public StaffLogin StaffLogin { get; set; }
            public List<StaffLoginRole> StaffLoginRoleList { get; set; }
            public int StaffLoginId { get; set; }
        }

        [HttpPost]
        [Route("SaveStaffLogin")]
        public ExpandoObject SaveStaffLogin(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            using (var dbContext = new AkashGangaTravelEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    SaveStaffLoginModel obj = JsonConvert.DeserializeObject<SaveStaffLoginModel>(decryptData);

                    var model = obj.StaffLogin;
                    StaffLogin StaffLogin = null;

                    // Remove existing roles first to avoid FK conflicts
                    var existingRoles = dbContext.StaffLoginRoles.Where(x => x.StaffLoginId == model.StaffLoginId).ToList();
                    dbContext.StaffLoginRoles.RemoveRange(existingRoles);
                    dbContext.SaveChanges();  // Save deletion before inserting

                    if (model.StaffLoginId > 0)
                    {
                        StaffLogin = dbContext.StaffLogins.FirstOrDefault(x => x.StaffLoginId == model.StaffLoginId);
                        StaffLogin.UpdatedBy = obj.StaffLoginId;
                        StaffLogin.UpdatedDate = DateTime.Now;
                    }
                    else
                    {
                        StaffLogin = new StaffLogin();
                        StaffLogin.CreatedBy = obj.StaffLoginId;
                        StaffLogin.CreatedDate = DateTime.Now;
                    }

                    StaffLogin.StaffId = model.StaffId;
                    StaffLogin.UserName = model.UserName;
                    StaffLogin.LoginPassword = model.LoginPassword;
                    StaffLogin.Status = model.Status;

                    dbContext.StaffLogins.AddOrUpdate(StaffLogin);
                    dbContext.SaveChanges();  // Ensure ID is available

                    dbContext.Entry(StaffLogin).Reload();  // Reload to get StaffLoginId

                    // Insert new roles
                    obj.StaffLoginRoleList.ForEach(role =>
                    {
                        if (role.StaffLoginRoleId == null)
                            role.StaffLoginRoleId = 0;

                        StaffLoginRole StaffLoginRole = new StaffLoginRole
                        {
                            StaffLoginId = StaffLogin.StaffLoginId,
                            RoleId = role.RoleId,
                            UpdatedBy = obj.StaffLoginId,
                            UpdatedDate = DateTime.Now
                        };

                        dbContext.StaffLoginRoles.Add(StaffLoginRole);
                    });

                    dbContext.SaveChanges();  // Save all role changes
                    transaction.Commit();  // Commit transaction before saving changes

                    response.Message = ConstantData.SuccessMessage;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    response.Message = $"Error: {ex.Message} | StackTrace: {ex.StackTrace}";
                }
            }
            return response;
        }


        [HttpPost]
        [Route("saveStaffLogins")]
        public ExpandoObject SaveStaffLogins(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
            DbTransaction transaction = null;
            int y = 0;
            try
            {
                transaction = dbContext.Database.Connection.BeginTransaction();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                SaveStaffLoginModel obj = JsonConvert.DeserializeObject<SaveStaffLoginModel>(decryptData);

                var model = obj.StaffLogin;
                StaffLogin StaffLogin = null;
                if (model.StaffLoginId > 0)
                {
                    StaffLogin = dbContext.StaffLogins.Where(x => x.StaffLoginId == model.StaffLoginId).First();
                    StaffLogin.UpdatedBy = obj.StaffLoginId;
                    StaffLogin.UpdatedDate = DateTime.Now;
                }
                else
                {
                    StaffLogin = new StaffLogin();
                    StaffLogin.CreatedBy = obj.StaffLoginId;
                    StaffLogin.CreatedDate = DateTime.Now;
                }
                StaffLogin.StaffId = model.StaffId;
                StaffLogin.UserName = model.UserName;
                StaffLogin.LoginPassword = model.LoginPassword;
                StaffLogin.Status = model.Status;
                if (StaffLogin.StaffLoginId == 0)
                    dbContext.StaffLogins.Add(StaffLogin);
                dbContext.SaveChanges();

                var deleteRole = dbContext.StaffLoginRoles.Where(x => x.StaffLoginId == StaffLogin.StaffLoginId && !obj.StaffLoginRoleList.Select(z => z.StaffLoginRoleId).Contains(x.StaffLoginRoleId));
                dbContext.StaffLoginRoles.RemoveRange(deleteRole);
                dbContext.SaveChanges();

                obj.StaffLoginRoleList.ForEach(role =>
                {
                    StaffLoginRole StaffLoginRole = null;
                    if (role.StaffLoginRoleId > 0)
                    {
                        StaffLoginRole = dbContext.StaffLoginRoles.Where(x => x.StaffLoginRoleId == role.StaffLoginRoleId).First();
                        StaffLoginRole.UpdatedBy = obj.StaffLoginId;
                        StaffLoginRole.UpdatedDate = DateTime.Now;
                    }
                    else
                        StaffLoginRole = new StaffLoginRole();
                    StaffLoginRole.StaffLoginId = StaffLogin.StaffLoginId;
                    StaffLoginRole.RoleId = role.RoleId;

                    if (StaffLoginRole.StaffLoginRoleId == 0)
                        dbContext.StaffLoginRoles.Add(StaffLoginRole);
                    dbContext.SaveChanges();
                });


                transaction.Commit();
                y = 1;
                response.Message = ConstantData.SuccessMessage;

            }
            catch (Exception ex)
            {
                if (y != 1)
                    transaction.Rollback();
                response.Message = ex.Message;
            }
            finally
            {
                if (null != dbContext.Database.Connection)
                    dbContext.Database.Connection.Close();
            }
            return response;
        }

        [HttpPost]
        [Route("deleteStaffLogin")]
        public ExpandoObject DeleteStaffLogin(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StaffLogin model = JsonConvert.DeserializeObject<StaffLogin>(decryptData);

                var StaffLogin = dbContext.StaffLogins.Where(x => x.StaffLoginId == model.StaffLoginId).First();
                dbContext.StaffLogins.Remove(StaffLogin);
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

        [HttpPost]
        [Route("StaffLogin")]
        public ExpandoObject StaffLogin(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
            LoginLog loginLog = new LoginLog();
            try
            {
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                StaffLogin model = JsonConvert.DeserializeObject<StaffLogin>(decryptData);

                loginLog.IPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                if (HttpContext.Current.Request.Browser.IsMobileDevice)
                    loginLog.LoginDevice = "Mobile";
                else
                    loginLog.LoginDevice = "Computer";
                if (HttpContext.Current.Request.UrlReferrer != null)
                    loginLog.ReferrerUrl = HttpContext.Current.Request.UrlReferrer.ToString();
                else
                    loginLog.ReferrerUrl = "";


                HttpBrowserCapabilities bcr = HttpContext.Current.Request.Browser;
                loginLog.UserName = model.UserName;
                loginLog.LoginPassword = (model.LoginPassword);
                //loginLog.LoginPassword = Password;

                loginLog.ClientBrowser = bcr.Browser;
                loginLog.LoginTime = DateTime.Now;
                loginLog.LastUpdatedOn = DateTime.Now;
                loginLog.LoginResult = (byte)LoginResult.Error;

                int countStaffLogin = (from e1 in dbContext.StaffLogins
                                       where (e1.UserName == model.UserName)
                                       select e1).Count();
                if (countStaffLogin <= 0)
                {
                    loginLog.LoginResult = (byte)LoginResult.WrongUserName;
                    throw new Exception("Invalid User Id or Password");
                }
                var employeeLogin = (from e1 in dbContext.StaffLogins
                                     where (e1.UserName == model.UserName)
                                     select e1).First();

                if (employeeLogin.Status == (byte)Status.Inactive)
                {
                    loginLog.LoginResult = (byte)LoginResult.AccountNotActive;
                    throw new Exception("Invalid User Id or Password");
                }
                if (employeeLogin.LoginPassword != model.LoginPassword)
                {
                    loginLog.LoginResult = (byte)LoginResult.WrongPassword;
                    throw new Exception("Invalid User Id or Password");
                }

                loginLog.LoginResult = (byte)LoginResult.Successful;

                response.UserDetail = (from d1 in dbContext.StaffLogins
                                       join s1 in dbContext.Staffs on d1.StaffId equals s1.StaffId
                                       join d2 in dbContext.Designations on s1.DesignationId equals d2.DesignationId
                                       join d3 in dbContext.StaffLoginRoles on d1.StaffLoginId equals d3.StaffLoginId
                                       where d1.StaffLoginId == employeeLogin.StaffLoginId
                                       select new
                                       {
                                           d1.StaffLoginId,
                                           d1.StaffId,
                                           s1.StaffName,
                                           d1.UserName,
                                           d2.DesignationName,
                                           d3.RoleId
                                       }).First();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                dbContext.LoginLogs.Add(loginLog);
                dbContext.SaveChanges();
                response.Message = ex.Message;
            }
            return response;
        }

       

        [HttpPost]
        [Route("changePassword")]
        public ExpandoObject ChangePassword(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                AkashGangaTravelEntities dbContext = new AkashGangaTravelEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ChangePasswordModel model = JsonConvert.DeserializeObject<ChangePasswordModel>(decryptData);

                StaffLogin StaffLogin = dbContext.StaffLogins.Where(x => x.StaffLoginId == model.Id).First();
                if (model.CurrentPassword != StaffLogin.LoginPassword)
                    throw new Exception("Invalid Current password!!");
                //if (CryptoEngine.Encrypt(model.CurrentPassword) != StaffLogin.LoginPassword)
                //    throw new Exception("Invalid Current password!!");
                StaffLogin.UpdatedDate = DateTime.Now;
                StaffLogin.UpdatedBy = model.Id;
                StaffLogin.LoginPassword = model.NewPassword;
                dbContext.SaveChanges();

                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}

