using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using QRCoder;
using WebCore.Services;

namespace Helper.User
{

    public class UserSession
    {
        public static void SetSession(UserSessionModel model)
        {
            HttpContext.Current.Session["loginSession"] = model;
        }
        public static UserSessionModel GetSession()
        {
             var sessionModel = HttpContext.Current.Session["loginSession"];
            if (sessionModel == null)
                return null;
            //
            return (UserSessionModel)sessionModel;
        }
    }

    [Serializable]
    public class UserSessionModel
    {
        public string IdentifyID { get; set; }
        public string LoginID { get; set; }
        public string SiteID { get; set; }
    }
    //
    public class InFormation
    {
        public static string GetFullName(string id)
        {
            UserInfoService userInfoService = new UserInfoService();
            string fullName = userInfoService.GetFullName(id);
            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;
            ///
            CMSUserInfoService cMSUserInfoService = new CMSUserInfoService();
            fullName = cMSUserInfoService.GetFullName(id);
            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;
            //
            return string.Empty;
        }
        public static string GetInfCreateBy(string id)
        {
            UserInfoService userInfoService = new UserInfoService();
            string fullName = userInfoService.GetFullName(id);
            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;
            ///
            CMSUserInfoService cMSUserInfoService = new CMSUserInfoService();
            fullName = cMSUserInfoService.GetFullName(id);
            if (!string.IsNullOrWhiteSpace(fullName))
                return "*:" + fullName;
            //
            return string.Empty;
        }
    }
    public class Access
    {

        public static bool IsLogin()
        {
            try
            {
                UserService userService = new UserService();
                return userService.IsLogin();
            }
            catch
            {
                return false;
            }
        }
        // static function ()

    }

    //
}