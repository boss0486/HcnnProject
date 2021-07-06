using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using System.Collections.Generic;
using WebCore.Model.Entities;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_ArticleGroup")]
    public partial class ArticleGroup : WEBModel
    {
        public ArticleGroup()
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
    public class ArticleGroupCreateModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public int Enabled { get; set; }
    }
    public class ArticleGroupUpdateModel : ArticleGroupCreateModel
    {
        public string ID { get; set; }
    }
    public class ArticleGroupIDModel
    {
        public string ID { get; set; }
    }
    public class ArticleGroupResult : WEBModelResult
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
    }
    public class ArticleGroupOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
}