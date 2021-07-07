using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using System.Collections.Generic;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_Faq")]
    public partial class Faq : WEBModel
    {
        public Faq()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string MenuID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
    }

    // model
    public class FaqCreateModel
    {
        public string MenuID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public int Enabled { get; set; }
    }
    public class FaqUpdateModel : FaqCreateModel
    {
        public string ID { get; set; }
    }
    public class FaqIDModel
    {
        public string ID { get; set; }
    }
    public class FaqResult : WEBModelResult
    {
        public string ID { get; set; }
        public string MenuID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Alias { get; set; }
        [NotMapped]
        public string MenuText => string.Empty;

    }
    public class FaqOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
}