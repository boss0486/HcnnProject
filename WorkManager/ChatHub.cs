using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using WebCore.Entities;
using WebCore.Services;



namespace WebApplication.Management.Controllers
{
    public class ChatHub : Hub
    {
        public List<string> ConnectionList = new List<string>();
        public string Connect(string id)
        {
            var idConnect = Context.ConnectionId;
            string logId = null;
            //if (string.IsNullOrWhiteSpace(logId))
            //    return Clients.All.sendResult(new ChatLogResult { Status = 400, Message = "::data invalid" });
            ////
            return Clients.All.connect(new ChatMessageResult { Status = 200, Message = "::connected" });
            //ChatConnectService chatConnectService = new ChatConnectService();
            //UserLoginService userLoginService = new UserLoginService();
            //UserLogin userLogin = userLoginService.GetAlls(m => m.ID == logId).FirstOrDefault();
            //// lay ra cac user can thong bao
            //if (userLogin == null)
            //    return Clients.Caller.connect(new ChatMessageResult { Status = 200, Message = "::not connected" });
            //// 
            //ChatConnect chatConnect = chatConnectService.GetAlls(m => m.UserID == logId).FirstOrDefault();
            //List<string> lstUser = chatConnectService.GetUserIDInRoom(logId);
            //if (chatConnect == null)
            //{
            //    string idChat = chatConnectService.Create<string>(new ChatConnect()
            //    {
            //        ConnectID = idConnect,
            //        UserID = logId
            //    });

            //    if (string.IsNullOrWhiteSpace(idChat))
            //        return Clients.Caller.connect(new ChatMessageResult { Status = 200, Message = "::not connected" });
            //    // 
            //    return Clients.Clients(lstUser).connect(new ChatMessageResult { Status = 200, Message = "::connected" });
            //}
            ////
            //chatConnect.ConnectID = idConnect;
            //chatConnect.UserID = logId;
            //chatConnectService.Update(chatConnect);
            //return Clients.Clients(lstUser).connect(new ChatMessageResult { Status = 200, Message = "::connected" });
        }
        public async Task<dynamic> JoinRoom(string roomId)
        {
            var idConnect = Context.ConnectionId;
            await Groups.Add(idConnect, roomId);
            return Clients.Caller.joinRoom(new ChatLogResult { Status = 200, Message = "::joined", RoomID= roomId });
        }
        public async Task<dynamic> SendMessage(ChatSendModel model)
        {
            var idConnect = Context.ConnectionId;
            try
            {
                if (model == null)
                    return null;
                // add room

                var loogedId = LoggedID();
                if (string.IsNullOrWhiteSpace(loogedId))
                    return Clients.Caller.sendResult(new ChatLogResult { Status = 400, Message = "::data invalid" });
                // 
                //await Groups.Add(idConnect, model.RoomID);
                // save message
                ChatMessageService chatMessageService = new ChatMessageService();
                string messageId = await chatMessageService.CreateAsync<string>(new ChatMessage
                {
                    RoomID = model.RoomID,
                    Message = model.Message,
                    CreatedBy = loogedId,
                    Enabled = 1, 
                });
                //
                if (string.IsNullOrWhiteSpace(messageId))
                    return Clients.Caller.sendResult(new ChatLogResult { Status = 400, Message = "::can not send message" });
                //
                return Clients.Group(model.RoomID).chatLog(new ChatLogResult { Status = 200, Message = "::sent" , RoomID = model.RoomID});
            }
            catch (Exception ex)
            {
                return Clients.Caller.sendResult(new ChatLogResult { Status = 500, Message = "::not service" });
            }
        }
        public string LoggedID()
        {
            Microsoft.AspNet.SignalR.Cookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            var _login = JsonConvert.DeserializeObject<CookiModel>(authTicket.UserData);
            if (_login == null)
                return null;
            //
            return _login.ID;
        }
        //        RoomID = "00001",
        //                WorkID = "00001",
        //                DateLog = DateTime.Now,
        //                Item = new LogItem
        //                {
        //                    UserID = "U01:" + model.UserID,
        //                    LogTime = "10:00",
        //                    Message = "Hello !"
        //                }
        //}
        //public class ChatHub : Hub
        //{
        //    public void Connect(string userId)
        //    {

        //        var idConnect = Context.ConnectionId;

        //        ChatConnectService chatConnectService = new ChatConnectService();
        //        WorkService workService = new WorkService();
        //        UserLoginService userLoginService = new UserLoginService();
        //        UserLogin userLogin = userLoginService.GetAlls(m => m.ID == userId).FirstOrDefault();

        //        // lay ra cac user can thong bao
        //        Clients.All.connect(new
        //        {
        //            ID = idConnect
        //        });

        //        if (userLogin == null)
        //        {
        //            Clients.All.connect(new ChatMessageResult
        //            {
        //                Status = 200,
        //                Message = "Lỗi kết nối"
        //            });
        //        }
        //        else
        //        {
        //            ChatConnect chatConnect = chatConnectService.GetAlls(m => m.UserID == userId).FirstOrDefault();
        //            if (chatConnect == null)
        //            {

        //                string idChat = chatConnectService.Create<string>(new ChatConnect()
        //                {
        //                    ConnectID = idConnect,
        //                    UserID = userId
        //                });
        //                if (string.IsNullOrWhiteSpace(idChat))
        //                {
        //                    Clients.User(idConnect).connect(new ChatMessageResult
        //                    {
        //                        Status = 200,
        //                        Message = "Lỗi kết nối"
        //                    });
        //                }
        //                else
        //                {
        //                    List<string> lstUser = chatConnectService.GetUserIDInRoom(userId);
        //                    Clients.Clients(lstUser).connect(new ChatMessageResult
        //                    {
        //                        Status = 200,
        //                        Message = "Ok"
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                chatConnect.ConnectID = idConnect;
        //                chatConnect.UserID = userId;
        //                chatConnectService.Update(chatConnect);
        //                //
        //                List<string> lstUser = chatConnectService.GetUserIDInRoom(userId);
        //                Clients.Clients(lstUser).connect(new ChatMessageResult
        //                {
        //                    Status = 200,
        //                    Message = "Ok"
        //                });
        //            }
        //        }
        //    }
        //    public void Send(ChatSendModel model)
        //    {
        //        Clients.All.sendResult(new ChatLogResult
        //        {
        //            RoomID = "00001",
        //            WorkID = "00001",
        //            DateLog = DateTime.Now,
        //            Item = new LogItem
        //            {
        //                UserID = "U01:" + model.UserID,
        //                LogTime = "10:00",
        //                Message = "Hello !"
        //            }
        //        });
        //    }
        //}



    }
    public class ChatSendModel
    {
        public string RoomID { get; set; }
        public string UserID { get; set; }
        public string Message { get; set; }
    }
    public class ChatLogResult
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string RoomID { get; set; }
    }

    public class LogItem
    {

        public string UserID { get; set; }
        public string LogTime { get; set; }
        public string Message { get; set; }
    }

    public class ChatMessageResult
    {

        public int Status { get; set; }
        public string Message { get; set; }
    }
}