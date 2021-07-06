//using AL.NetFrame.Attributes;
//using AL.NetFrame.Interfaces;
//using AL.NetFrame.Services;
//using Dapper;
//using System;
//using System.Collections.Generic;
//using System.Web;
//using System.Web.Mvc;
//using WebCore.Model.Entities;
//namespace WebCore.Entities
//{

//    [ConnectionString(DbConnect.ConnectionString.CMS)]
//    [Table("UserSetting")]
//    public partial class UserSetting : WEBModel
//    {
//        public UserSetting()
//        {
//            ID = Guid.NewGuid().ToString().ToLower();
//        }

//        [Key]
//        [IgnoreUpdate]
//        public string ID { get; set; }
//        public string UserID { get; set; }
//        public string SecurityPassword { get; set; }
//        public string AuthenType { get; set; }
//        public bool IsBlock { get; set; }
//        public bool IsRootUser { get; set; }
//        public string DepartmentID { get; set; }
//        public string DepartmentPartID { get; set; }

//    }
//}

using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using Helper.Language;
using Helper.TimeData;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("UserSetting")]
    public partial class UserSetting:WEBModel
    {
        public UserSetting()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string UserID { get; set; }
        public string SecurityPassword { get; set; }
        public string AuthenType { get; set; }
        public bool IsBlock { get; set; }
        public bool IsRootUser { get; set; }
        public string DepartmentID { get; set; }
        public string DepartmentPartID { get; set; }
        public int DepartmentLevel { get; set; }
        public int Status { get; set; }
    }
}