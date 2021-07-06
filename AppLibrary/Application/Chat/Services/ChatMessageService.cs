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
    public interface IChatMessageService : IEntityService<ChatMessage> { }
    public class ChatMessageService : EntityService<ChatMessage>, IChatMessageService
    {
        public ChatMessageService() : base() { }
        public ChatMessageService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################

        public ActionResult ChatLog(ChatLogModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            int page = model.Page;
            //string query = model.Query;
            //if (string.IsNullOrWhiteSpace(query))
            //    query = "";
            //
            string whereCondition = string.Empty;
            //
            //SearchResult searchResult = WebCore.Model.Services.ModelService.SearchDefault(new SearchModel
            //{
            //    Query = model.Query,
            //    TimeExpress = model.TimeExpress,
            //    Status = model.Status,
            //    StartDate = model.StartDate,
            //    EndDate = model.EndDate,
            //    Page = model.Page,
            //    TimeZoneLocal = model.TimeZoneLocal
            //});
            //if (searchResult != null)
            //{
            //    if (searchResult.Status == 1)
            //        whereCondition = searchResult.Message;
            //    else
            //        return Notifizsation.Invalid(searchResult.Message);
            //}
            // 
            string roomId = model.RoomID;
            if (string.IsNullOrWhiteSpace(roomId))
                return Notifization.NotFound(MessageText.NotFound);
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_ChatMessage WHERE RoomID = @RoomID AND CreatedDate >= DATEADD(month, -6, GETDATE()) ORDER BY [CreatedDate] ASC";
            List<ChatLogData> dtList = _connection.Query<ChatLogData>(sqlQuery, new { RoomID = roomId }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ChatLogResult> dtResult = new List<ChatLogResult>();
            List<string> dtDate = dtList.GroupBy(m => m.DataDate).Select(m => m.Key).ToList();
            foreach (var item in dtDate)
            {
                dtResult.Add(new ChatLogResult
                {
                    DataDate = item,
                    Items = dtList.Where(m => m.DataDate == item).ToList()
                });
            }
            return Notifization.Data(MessageText.Success, data: dtResult);
        }

        public ActionResult ChatNotify()
        {
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT Top 20 FROM App_ChatMessage WHERE ID !=""  ORDER BY [CreatedDate] ASC";
            List<ChatLogData> dtList = _connection.Query<ChatLogData>(sqlQuery, new
            {
                UserID = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            //
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ChatLogResult> dtResult = new List<ChatLogResult>();
            List<string> dtDate = dtList.GroupBy(m => m.DataDate).Select(m => m.Key).ToList();
            foreach (var item in dtDate)
            {
                dtResult.Add(new ChatLogResult
                {
                    DataDate = item,
                    Items = dtList.Where(m => m.DataDate == item).ToList()
                });
            }
            return Notifization.Data(MessageText.Success, data: dtResult);
        }
        //##############################################################################################################################################################################################################################################################
        public static string DataDate(DateTime dateTime)
        {
            return dateTime.ToString("dd-MM-yyyy");
        }
        public static string DataTime(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm");
        }
    }
}