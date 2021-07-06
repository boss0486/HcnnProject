using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using SignalRChat.Models;
using WebCore.Entities;
using WebCore.Services;

namespace WebApplication.Management.Controllers
{
    [RouteArea("Management")]
    [RoutePrefix("Chat")]
    public class ChatController : Controller
    {
        // GET: Chat
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Chat()
        {
            return View();
        }

        [HttpPost]
        [Route("Action/ChatLog")] 
        public ActionResult ChatLog(ChatLogModel model)
        {
            try
            {
                using (var service = new ChatMessageService())
                    return service.ChatLog(model);
                //
            }
            catch (Exception ex)
            {
                return Helper.Notifization.NotService;
            }

        }
        [HttpPost]
        [Route("Action/Notify")]
        public ActionResult ChatNotify()
        {
            try
            {
                using (var service = new ChatMessageService())
                    return service.ChatNotify();
                //
            }
            catch (Exception ex)
            {
                return Helper.Notifization.NotService;
            }

        }
    }
}