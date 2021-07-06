﻿using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using WebCore.Model.Entities;
namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("Role")]
    public partial class Role : WEBModel
    {
        public Role()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
        public int OrderID { get; set; }
    }
    public partial class RoleResult : WEBModelResult
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
        public int OrderID { get; set; }

        public List<RoleResult> SubRoles { get; set; }

    }

    // model
    public class RoleCreateModel : WEBModel
    {
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public int OrderID { get; set; }

    }
    public class RoleUpdateModel : RoleCreateModel
    {
        public string ID { get; set; }
    }
    public class RoleIDModel
    {
        public string ID { get; set; }
    }

    public class RoleOption
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public int OrderID { get; set; }
        public string Title { get; set; }
        [NotMapped]
        public List<RoleOption> SubOption { get; set; }
    }
    public class RoleOptionForUser
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public int OrderID { get; set; }
        public string Title { get; set; }
        public bool Active { get; set; }
        [NotMapped]
        public List<RoleOptionForUser> SubOption { get; set; }

    }
    public class SubRoleCategory
    {
        public string InnerText { get; set; }
        public bool IsSubNull { get; set; }

    }
}