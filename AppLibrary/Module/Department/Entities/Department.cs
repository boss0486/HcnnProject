using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using WebCore.Model.Entities;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_Department")]
    public partial class Department : WEBModel
    {
        public Department()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }

    }

    // model
    public class DepartmentCreateModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public int Enabled { get; set; }

    }
    public class DepartmentUpdateModel : DepartmentCreateModel
    {
        public string ID { get; set; }
    }
    public class DepartmentIDModel
    {
        public string ID { get; set; }
    }
    public class DepartmentResult : WEBModelResult
    {
        DepartmentResult()
        {
            Summary = string.Empty;
        }
        public string ID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }  
        public string Alias { get; set; }
    }
    public class DepartmentOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
    public class DepartmentLevelOption
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
}