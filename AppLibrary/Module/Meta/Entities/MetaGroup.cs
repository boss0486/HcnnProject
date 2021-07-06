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
    [Table("App_MetaGroup")]
    public partial class MetaGroup : WEBModel
    {
        public MetaGroup()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }

        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string Alias { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
    }


    // model
    public class MetaGroupCreateModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public int Enabled { get; set; }
    }
    public class MetaGroupUpdateModel : MetaGroupCreateModel
    {
        public string ID { get; set; }
    }
    public class MetaGroupIDModel
    {
        public string ID { get; set; }
    }
    public class MetaGroupResult : WEBModelResult
    {

        public string ID { get; set; }
        public string Alias { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
 
    }
    public class MetaGroupOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
}