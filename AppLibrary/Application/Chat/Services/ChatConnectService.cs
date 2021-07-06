using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using System;
using Dapper;
using System.Linq;
using PagedList;
using System.Web.Mvc;
using System.Collections.Generic;
using Helper;
using System.Web;
using WebCore.Entities;
using WebCore.Model.Entities;
using WebCore.ENM;
using WebCore.Model.Enum;
using Helper.Page;

namespace WebCore.Services
{
    public interface IChatConnectService : IEntityService<ChatConnect> { }
    public class ChatConnectService : EntityService<ChatConnect>, IChatConnectService
    {
        public ChatConnectService() : base() { }
        public ChatConnectService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################
        public List<string> GetUserIDInRoom(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"  SELECT c.ConnectID FROM (SELECT  * FROM  App_Work WHERE SiteID = @SiteID AND (ExecuteUserID = @ID OR AssignorUserID = @ID OR FollowerUserID = @ID)) as w
            unpivot (value1 for col in (ExecuteUserID,AssignorUserID,FollowerUserID))un 
            INNER JOIN  App_ChatConnect c ON c.UserID = un.value1 
            GROUP BY c.ConnectID";
            List<string> dataList = _connection.Query<string>(sqlQuery, new
            {
                ID = id,
                SiteID = "6f110fb6-bcd2-469a-a0bc-9e2469f4ada1"
            }).ToList();
            if (dataList == null)
                return null;
            //
            return dataList;
        }


    }
}