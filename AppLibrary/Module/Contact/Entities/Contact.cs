using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using WebCore.Model.Entities;
using WebCore.Model.Services;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_Contact")]
    public class Contact : WEBModel
    {
        public Contact()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Alias { get; set; }
        public string Content { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int State { get; set; }
    }

    public class ContactCreateModel
    {
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Content { get; set; }
        public bool State { get; set; }
        public int Enabled { get; set; }
    }
    public class ContactUpdateModel : ContactCreateModel
    {
        public string ID { get; set; }
    }
    public class ContactIDModel
    {
        public string ID { get; set; }
    }
    public class ContactResult : WEBModelResult
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool State { get; set; }
        [NotMapped]
        public string StateText => ContactService.ViewContactState(State);
    }
}
