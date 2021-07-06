using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using WebCore.Model.Entities;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_Support")]
    public partial class Support : WEBModel
    {
        public Support()
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
    public class SupportCreateModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public int Enabled { get; set; }

    }
    public class SupportUpdateModel : BankCreateModel
    {
        public string ID { get; set; }
    }
    public class SupportIDModel
    {
        public string ID { get; set; }
    }
    public class SupportResult : WEBModelResult
    {

        public string ID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
    }
    public class SupportOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
}