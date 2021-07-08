using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.Security;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Services;
using Helper.User;
using Dapper;

namespace Helper.Current
{
    public class UserLogin : IHttpHandler, IRequiresSessionState
    {
        public static string LanguageID
        {
            get
            {
                UserService service = new UserService();
                return service.GetLanguageID;
            }
        }

        public static int UserRoleInApplication
        {
            get
            {
                try
                {
                    if (Helper.Current.UserLogin.IsCMSUser)
                        return 1;
                    if (Helper.Current.UserLogin.IsAdminInApplication)
                        return 2;
                    //
                    return 0;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        public static bool IsCMSUser
        {
            get
            {
                try
                {
                    var service = new AuthenService();
                    var logged = service.LoggedModel();
                    if (logged == null)
                        return false;
                    //
                    return logged.IsCMSUser;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool IsApplicationUser
        {
            get
            {
                var service = new AuthenService();
                var logged = service.LoggedModel();
                if (logged != null)
                    return logged.IsApplication;
                //
                return false;
            }
        }

        public static bool IsAdminInApplication
        {
            get
            {
                var service = new AuthenService();
                var logged = service.LoggedModel();
                if (logged != null)
                    return logged.IsAdministrator;
                //
                return false;
            }
        }


        public static int UserLevel
        {
            get
            {
                AuthenService service = new AuthenService();
                var logged = service.LoggedModel();
                UserSettingService userSettingService = new UserSettingService();
                UserSetting userSetting = userSettingService.GetAlls(m => m.UserID == Helper.Current.UserLogin.IdentifierID).FirstOrDefault();
                if (userSetting != null)
                {
                    return userSetting.DepartmentLevel;
                }
                return 0;
            }
        }

        public static Logged LoggedModel
        {
            get
            {
                try
                {
                    var service = new AuthenService();
                    var logged = service.LoggedModel();
                    return logged;
                }
                catch (Exception)
                {
                    return new Logged();
                }
            }
        }
        //
        public static LoginInForModel LoginInFor()
        {
            try
            {
                string userId = Helper.Current.UserLogin.IdentifierID;
                using (var service = new UserService())
                    return service.LoginInformation(userId);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string IdentifierID
        {
            get
            {
                try
                {
                    if (UserSession.GetSession() == null)
                        return string.Empty;
                    //
                    return UserSession.GetSession().IdentifyID;
                }
                catch (Exception)
                {
                    return string.Empty;
                }

            }
        }

        public static string SiteID
        {
            get
            {
                try
                {
                    if (UserSession.GetSession() == null)
                        return string.Empty;
                    return UserSession.GetSession().SiteID;
                }
                catch (Exception)
                {
                    return string.Empty;
                }

            }
        }

        public static string LoginID
        {
            get
            {
                if (UserSession.GetSession() == null)
                    return string.Empty;
                return UserSession.GetSession().LoginID;
            }
        }

        // ###########################################################################################################################################################################################
        public static DateTime Now => DateTime.Now;
        // ###########################################################################################################################################################################################

        public bool IsReusable => throw new NotImplementedException();

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
