using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using System;
using Dapper;
using System.Linq;
using PagedList;
using Helper;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using WebCore.Model.Enum;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Services;
using Helper.Page;
using Helper.File;
using WebCore.Model.Entities;
using WebCore.Model.Services;
using System.Data;
using System.Web.Security;
using WebCore.ENM;
using Helper.TimeData;

namespace WebCore.Services
{
    public interface IAuthenService : IEntityService<DbConnection> { }
    public class AuthenService : EntityService<DbConnection>, IAuthenService
    {

        public AuthenService() : base() { }
        public AuthenService(System.Data.IDbConnection db) : base(db) { }
        //###############################################################################################################################
        public ActionResult Login(LoginReqestModel model)
        {
            using (var services = new UserService())
            {
                if (model == null)
                    return Notifization.Invalid(MessageText.Invalid);
                //
                string loginId = model.LoginID;
                string password = model.Password;
                if (string.IsNullOrWhiteSpace(loginId) || string.IsNullOrWhiteSpace(password))
                    return Notifization.Invalid(MessageText.Invalid);
                //   
                var sqlHelper = DbConnect.Connection.CMS;
                var login = _connection.Query<Logged>("sp_user_login", new { model.LoginID, Password = Helper.Security.Library.Encryption256(password) }, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();
                //
                if (login == null)
                    return Notifization.Invalid("Sai khoản hoặc mật khẩu");
                //
                if (login.Enabled != (int)ModelEnum.State.ENABLED)
                    return Notifization.Invalid("Tài khoản chưa được kích hoạt");
                //
                if (login.IsBlock)
                    return Notifization.Invalid("Tài khoản của bạn bị khóa");
                //

                Helper.User.UserSession.SetSession(new Helper.User.UserSessionModel
                {
                    IdentifyID = login.ID,
                    LoginID = login.LoginID,
                    SiteID = login.SiteID
                });
                // delete cookies
                HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddDays(-1);
                // set cooki
                string _uData = "{'ID':'','LoginID': '', 'Password': '', 'IsRemember': 0} ";
                if (model.IsRemember)
                    _uData = "{'ID':'"+ login.ID +"', 'LoginID': '" + model.LoginID + "', 'Password': '" + model.Password + "', 'IsRemember': 1} ";

                bool isPersistent = false;
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                    login.ID,
                    DateTime.Now,
                    DateTime.Now.AddDays(10),
                    isPersistent,
                    _uData,
                    FormsAuthentication.FormsCookiePath
                );
                // Encrypt the ticket.
                string encTicket = FormsAuthentication.Encrypt(ticket);
                // Create the cookie.
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket)); 
                string prePath = Helper.Page.Navigate.NavigateByParam(model.Url);
                // Development
                if (login.IsCMSUser)
                { 
                    // default
                    if (string.IsNullOrWhiteSpace(prePath))
                        prePath = "/Development/Home/index";
                    //retun
                    prePath = prePath.ToLower();
                    return Notifization.Success("Đăng nhập thành công", prePath);
                } 
                // something here  
                if (string.IsNullOrWhiteSpace(prePath))
                    prePath = "/Management/Home/Index";
                //retun
                prePath = prePath.ToLower();
                return Notifization.Success("Đăng nhập thành công", prePath);
            }
        }

        public ActionResult LoginQR(LoginQRReqestModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string loginId = model.LoginID;
            string pinCode = model.PinCode;
            if (string.IsNullOrWhiteSpace(loginId) && string.IsNullOrWhiteSpace(pinCode))
                return Notifization.Invalid();
            //   
            var sqlHelper = DbConnect.Connection.CMS;
            string sqlQuerry = @"SELECT TOP 1 *,IsCMSUser = 1 FROM View_CMSUserLogin WHERE LoginID = @LoginID AND PinCode = @PinCode
                                     UNION 
                                     SELECT TOP 1 *,IsCMSUser = 0 FROM View_UserLogin WHERE LoginID = @LoginID AND PinCode = @PinCode";
            var login = sqlHelper.Query<Logged>(sqlQuerry, new { model.LoginID, PinCode = Helper.Security.Library.Encryption256(pinCode) }).FirstOrDefault();
            //
            if (login == null)
                return Notifization.Invalid("Sai thông tin tài khoản hoặc mật khẩu");
            //
            if (login.Enabled != (int)ModelEnum.State.ENABLED)
                return Notifization.Invalid("Tài khoản chưa được kích hoạt");
            //
            if (login.IsBlock)
                return Notifization.Invalid("Tài khoản của bạn bị khóa");
            //
            //HttpContext.Current.Session["IdentifyID"] = login.ID;
            //HttpContext.Current.Session["LoginID"] = login.LoginID;
            // delete cookies
            //HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddDays(-1);
            //// set cooki
            //string _uData = "{'LoginID': '', 'Password': '', 'IsRemember': 0} ";
            //if (model.IsRemember)
            //    _uData = "{'LoginID': '" + model.LoginID + "', 'Password': '" + model.Pu + "', 'IsRemember': 1} ";
            //bool isPersistent = false;
            //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
            //    model.LoginID,
            //    DateTime.Now,
            //    DateTime.Now.AddDays(10),
            //    isPersistent,
            //    _uData,
            //    FormsAuthentication.FormsCookiePath
            //);
            // Encrypt the ticket.
            //string encTicket = FormsAuthentication.Encrypt(ticket);
            // Create the cookie.
            //HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
            //navigate by account type
            // get pre-page 
            string prePath = Helper.Page.Navigate.NavigateByParam(model.Url);
            // Development
            if (login.IsCMSUser)
            {
                // check permission
                // something here
                // default
                if (string.IsNullOrWhiteSpace(prePath))
                    prePath = "/Development/Home/index";
                //retun
                prePath = prePath.ToLower();
                return Notifization.Success("Đăng nhập thành công", prePath);
            }
            // Application
            // check permission
            // something here

            // default
            if (string.IsNullOrWhiteSpace(prePath))
                prePath = "/Management/Home/Index";
            //retun
            prePath = prePath.ToLower();
            return Notifization.Success("Đăng nhập thành công", prePath);

        }

        public Logged LoggedModel(IDbConnection dbConnection = null, IDbTransaction transaction = null)
        {
            try
            {
                if (dbConnection == null)
                    dbConnection = _connection;
                //
                string loginId = Helper.Current.UserLogin.LoginID;
                string sqlQuerry = @"SELECT TOP 1 *,IsCMSUser = 1, IsApplication = 0 FROM View_CMSUserLogin WHERE LoginID = @LoginID 
                                     UNION 
                                     SELECT TOP 1 *,IsCMSUser = 0, IsApplication = 1 FROM View_UserLogin WHERE LoginID = @LoginID ";
                var logged = dbConnection.Query<Logged>(sqlQuerry, new { LoginID = loginId }, transaction: transaction).FirstOrDefault();
                if (logged == null)
                    return null;
                //
                return logged;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult ForgotPassword(UserEmailModel model)
        {

            string strEmail = model.Email.ToLower();
            string sqlQuerry = @"SELECT TOP 1 *,IsCMSUser = 1 FROM View_CMSUserLogin WHERE Email = @Email 
                                 UNION 
                                 SELECT TOP 1 *,IsCMSUser = 0 FROM View_UserLogin WHERE Email = @Email";
            var user = _connection.Query<Logged>(sqlQuerry, new { Email = model.Email }).FirstOrDefault();
            //
            if (user == null)
                return Notifization.NotFound("Dữ liệu không hợp lệ");
            // send mail
            string strOTP = Helper.Security.Library.OTPCode;
            string strGuid = new Guid().ToString();
            string strToken = Helper.Security.HashToken.Create(user.LoginID);
            //  send otp for reset password 
            string subject = "XÁC THỰC OTP";
            int status = Helper.Email.MailService.SendOTP_ForGotPassword(strEmail, subject, strOTP);
            if (status != 1)
                return Notifization.Invalid("Không thể gửi mã OTP tới email của bạn");
            //
            if (user.IsCMSUser)
            {
                CMSUserLoginService cmsUserLoginService = new CMSUserLoginService(_connection);
                CMSUserLogin cmsUserLogin = cmsUserLoginService.GetAlls(m => m.ID == user.ID).FirstOrDefault();
                cmsUserLogin.OTPCode = strOTP;
                cmsUserLogin.TokenID = strToken;
                cmsUserLoginService.Update(cmsUserLogin);
            }
            else
            {
                UserLoginService userLoginService = new UserLoginService(_connection);
                UserLogin userLogin = userLoginService.GetAlls(m => m.ID == user.ID).FirstOrDefault();
                userLogin.OTPCode = strOTP;
                userLogin.TokenID = strToken;
                userLoginService.Update(userLogin);
            }
            return Notifization.Success("Mã OTP đã được gửi tới email của bạn", "/Authentication/ResetPassword?token=" + strToken);
        }
        public ActionResult ResetPassword(UserResetPasswordModel model)
        {
            if (model == null)
                return Notifization.Invalid();

            string otp = model.OTPCode;
            string password = model.Password;

            // password
            if (string.IsNullOrWhiteSpace(password))
                return Notifization.Invalid("Không được để trống mật khẩu");
            if (!Validate.TestPassword(password))
                return Notifization.Invalid("Yêu cầu mật khẩu bảo mật hơn");
            if (password.Length < 4 || password.Length > 16)
                return Notifization.Invalid("Mật khẩu giới hạn [4-16] ký tự");
            //
            if (string.IsNullOrWhiteSpace(model.TokenID))
                return Notifization.Invalid("Dữ liệu không hợp lệ");
            // a sample jwt encoded token string which is supposed to be extracted from 'Authorization' HTTP header in your Web Api controller
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(model.TokenID);
            var token = handler.ReadToken(model.TokenID) as JwtSecurityToken;
            string loginId = token.Claims.First(c => c.Type == "TokenID").Value;
            string tokenKey = token.Claims.First(c => c.Type == "TokenKey").Value;
            string tokenTime = token.Claims.First(c => c.Type == "TokenTime").Value;
            if (string.IsNullOrWhiteSpace(loginId))
                return Notifization.UnAuthorized;
            //
            string sqlQuerry = @"SELECT TOP 1 *,IsCMSUser = 1 FROM View_CMSUserLogin WHERE LoginID = @LoginID
                                 UNION 
                                 SELECT TOP 1 *,IsCMSUser = 0 FROM View_UserLogin WHERE LoginID = @LoginID";
            var user = _connection.Query<Logged>(sqlQuerry, new { LoginID = loginId }).FirstOrDefault();
            //
            if (user == null)
                return Notifization.NotFound();
            //
            if (user.IsCMSUser)
            {
                CMSUserLoginService cmsUserLoginService = new CMSUserLoginService(_connection);
                CMSUserLogin cmsUserLogin = cmsUserLoginService.GetAlls(m => !string.IsNullOrWhiteSpace(m.LoginID) && m.LoginID.ToLower() == loginId.ToLower()).FirstOrDefault();
                if (cmsUserLogin == null)
                    return Notifization.Invalid("Dữ liệu không hợp lệ");
                //
                if (cmsUserLogin.OTPCode != otp)
                    return Notifization.Invalid("Mã OTP chưa đúng");
                //
                cmsUserLogin.OTPCode = string.Empty;
                cmsUserLogin.TokenID = string.Empty;
                cmsUserLogin.Password = Helper.Security.Library.Encryption256(password);
                cmsUserLoginService.Update(cmsUserLogin);
            }
            else
            {
                UserLoginService userLoginService = new UserLoginService(_connection);
                UserLogin userLogin = userLoginService.GetAlls(m => !string.IsNullOrWhiteSpace(m.LoginID) && m.LoginID.ToLower() == loginId.ToLower()).FirstOrDefault();
                if (userLogin == null)
                    return Notifization.Invalid("Dữ liệu không hợp lệ");
                //
                if (userLogin.OTPCode != otp)
                    return Notifization.Invalid("Sai mã OTP");
                //
                userLogin.OTPCode = string.Empty;
                userLogin.TokenID = string.Empty;
                userLogin.Password = Helper.Security.Library.Encryption256(password);
                userLoginService.Update(userLogin);
            }
            return Notifization.Success(MessageText.UpdateSuccess, "/Authentication");
        }
    }
}
