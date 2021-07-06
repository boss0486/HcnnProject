﻿using AL.NetFrame.Attributes;
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
    [Table("App_WorkAssign")]
    public partial class WorkStack
    {
        public WorkStack()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string WorkID { get; set; }
        public string AssignTo { get; set; }
        public string ParentID { get; set; }
        public int AssignType { get; set; }
        public DateTime AssignDate { get; set; }
    }
}