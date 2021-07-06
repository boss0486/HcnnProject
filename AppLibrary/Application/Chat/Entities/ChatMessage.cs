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
    [Table("App_ChatMessage")]
    public partial class ChatMessage : WEBModel
    {
        public ChatMessage()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string RoomID { get; set; }
        public string Summary { get; set; }
        public string Message { get; set; }
    }

    public partial class ChatLogModel
    {
        public string RoomID { get; set; }
        public int Page { get; set; }
    }
    public partial class ChatLogData
    {
        public string ID { get; set; }
        public string RoomID { get; set; }
        public string Summary { get; set; }
        public string Message { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        public string Name => Helper.User.InFormation.GetInfCreateBy(CreatedBy);
        [NotMapped]
        public string DataTime => ChatMessageService.DataTime(CreatedDate);
        [NotMapped]
        public string DataDate => ChatMessageService.DataDate(CreatedDate);
    }

    public partial class ChatLogResult
    { 
        public string DataDate { get; set; }
        public List<ChatLogData> Items { get; set; }
         
    }
}


