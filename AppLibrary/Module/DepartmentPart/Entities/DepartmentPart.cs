using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_DepartmentPart")]
    public partial class DepartmentPart : WEBModel
    {
        public DepartmentPart()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string DepartmentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }

    }

    // model
    public class DepartmentPartCreateModel
    {
        public string DepartmentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public int Enabled { get; set; }

    }
    public class DepartmentPartUpdateModel : DepartmentPartCreateModel
    {
        public string ID { get; set; }
    }
    public class DepartmentPartIDModel
    {
        public string ID { get; set; }
    }    
    
    public class DepartmentPartDepartmentIDModel
    {
        public string DepartmentID { get; set; }
    }
    public class DepartmentPartResult : WEBModelResult
    {
        DepartmentPartResult()
        {
            Summary = string.Empty;
        }
        public string ID { get; set; }
        public string DepartmentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
        [NotMapped]
        public string DepartmentName => DepartmentService.GetDepartmentNameByID(DepartmentID);
        
    }
    public class DepartmentPartOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
}