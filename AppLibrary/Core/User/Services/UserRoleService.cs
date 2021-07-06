using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using WebCore.Entities;
using System;
using Dapper;
using System.Linq;
using PagedList;
using Helper;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web;
namespace WebCore.Services
{
    public interface IUserRole : IEntityService<UserRole> { }
    public class UserRoleService : EntityService<UserRole>, IUserRole
    {
        public UserRoleService() : base() { }
        public UserRoleService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################
        public static List<string> GetUserRoleByUserID(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<string>();
            //
            userId = userId.ToLower();
            UserRoleService userRoleService = new UserRoleService();
            List<string> roles = userRoleService.GetAlls(m => !string.IsNullOrWhiteSpace(m.UserID) && m.UserID == userId).Select(m => m.RoleID).ToList();
            if (roles.Count == 0)
                return new List<string>();
            //
            return roles;
        }
        //##############################################################################################################################################################################################################################################################
    }
}