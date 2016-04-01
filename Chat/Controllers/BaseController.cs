using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using EC2.Chat.Models;
using EC2.ChatEngine;
using System.Resources;
using System.Threading;
using System.Globalization;
using EC2.Chat.Helpers;

namespace EC2.Chat.Controllers
{
    [CultureAttribute]
    public class BaseController<T> : Controller
        where T : IChatroom, new()
    {
        protected ChatManager<T> manager = null;

        
        
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            manager = HttpContext.Application["ChatManager"] as ChatManager<T>;

            if (manager == null)
            {
                manager = new ChatManager<T>();
                HttpContext.Application["ChatManager"] = manager;
            }
        }

        public void Save()
        {
            HttpContext.Application["ChatManager"] = manager;
        }

        public void Warning(string message)
        {
            if (!TempData.Keys.Contains(Message.Status.Warning.ToString()))
                TempData.Add(Message.Status.Warning.ToString(), message);
            else
                TempData[Message.Status.Warning.ToString()] = TempData[Message.Status.Warning.ToString()].ToString() + "<br />" + message;
        }

        public void Information(string message)
        {
            if (!TempData.Keys.Contains(Message.Status.Info.ToString()))
                TempData.Add(Message.Status.Info.ToString(), message);
            else
                TempData[Message.Status.Info.ToString()] = TempData[Message.Status.Info.ToString()].ToString() + "<br />" + message;
        }

        public void Error(string message)
        {
            if (!TempData.Keys.Contains(Message.Status.Error.ToString()))
                TempData.Add(Message.Status.Error.ToString(), message);
            else
                TempData[Message.Status.Error.ToString()] = TempData[Message.Status.Error.ToString()].ToString() + "<br />" + message;
        }

        //public Chatroom GetRoom(Guid roomId)
        //{
        //    return HttpContext.Application[roomId.ToString()] as Chatroom;
        //}

        //public void DeleteRoom(Guid roomId)
        //{
        //    HttpContext.Application.Remove(roomId.ToString());
        //}

        //public void AddRoom(Chatroom room)
        //{
        //    HttpContext.Application[room.ChatroomID.ToString()] = room;
        //}



    }
}
