using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using WebCore.Model.Entities;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_SocialNetwork")]
    public partial class SocialNetwork : WEBModel
    {
        public SocialNetwork()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        public int IconID { get; set; }
        public string BackLink { get; set; }

    }

    // model
    public class SocialNetworkCreateModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public int IconID { get; set; }
        public string BackLink { get; set; }
        public int Enabled { get; set; }

    }
    public class SocialNetworkUpdateModel : SocialNetworkCreateModel
    {
        public string ID { get; set; }
    }
    public class SocialNetworkIDModel
    {
        public string ID { get; set; }
    }
    public class SocialNetworkResult : WEBModelResult
    {

        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; } = string.Empty;
        public int IconID { get; set; }
        public string BackLink { get; set; }
    }
    public class SocialNetworkOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }
    public class SocialNetworkIcon
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }

        public SocialNetworkIcon(int _id,string _title, string _icon)
        {
            ID = _id;
            Title = _title;
            Icon = _icon;
        }
    }
}