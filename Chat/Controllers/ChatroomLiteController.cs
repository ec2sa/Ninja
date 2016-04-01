using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Chat;
using EC2.Chat.Models;
using EC2.ChatEngine;
using System.Text.RegularExpressions;

namespace EC2.Chat.Controllers
{
    public class ChatroomLiteController : BaseController<ChatroomLite>
    {
        private IChatroom room = null;


        public ActionResult Create()
        {
            ChatroomViewModel model = new ChatroomViewModel();
            return View(model);
        }


        [HttpPost]
        public ActionResult Create(string Name, string Nick)
        {
            if (string.IsNullOrWhiteSpace(Name)) Name = "Anonymous";
            if (string.IsNullOrWhiteSpace(Nick)) Nick = "Anonymous";

            ChatResult result = null;
            room = manager.Add(Name, Nick, out result);
            TempData["PIN"] = result.Pin;

            Save();
            return RedirectToAction("Details", new { roomId = room.ChatroomID });
        }

        [HttpPost]
        public ActionResult Join(Guid? roomId, string Nick)
        {
            if (string.IsNullOrWhiteSpace(Nick)) Nick = "Anonymous";

            if (roomId.HasValue)
            {
                var room = manager.Get(roomId.Value) as ChatroomLite;

                if (room != null)
                {
                    ChatResult result = room.Join(Nick);
                    if (result.Terminate == true)
                    {
                        Error("Nieprawidłowy PIN lub PIN już został wykorzystany");
                        return RedirectToAction("Index", "Home");
                    }

                    Save();

                    TempData["PIN"] = result.Pin;


                    return RedirectToAction("Details", new { roomId = room.ChatroomID });
                }
                else
                {
                    Error("Chat o podanym adresie nie istnieje!");
                    return RedirectToAction("Index", "Home");
                }
            }

            Error("Chat o podanym adresie nie istnieje!");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Leave(Guid? roomId, string PIN)
        {
            if (roomId.HasValue)
            {
                var room = manager.Get(roomId.Value);
                if (room != null)
                {
                    ChatResult result = room.Leave(PIN);

                    Save();
                }

                Information("Wylogowano pomyślnie");

                return RedirectToAction("Index", "Home");
            }

            Error("Użytkownik o podanym numerze PIN nie istnieje!");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult DeleteRoom(Guid roomId, string PIN)
        {
            ChatResult result = manager.Delete(roomId, PIN);

            Save();

            Error("Chat został zamknięty!");
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Details(Guid? roomId)
        {
            if (roomId.HasValue)
            {
                IChatroom room = manager.Get(roomId.Value);
                ChatroomViewModel model = new ChatroomViewModel();
                if (room != null)
                {
                    model.Chatroom = room;
                    model.roomId = roomId.Value;

                    string PIN = TempData["PIN"] as string;
                    TempData.Remove("PIN");
                    if (!string.IsNullOrWhiteSpace(PIN))
                    {
                        model.PIN = PIN;
                        model.Users = room.GetUsers(PIN);
                        model.IsOwner = room.IsOwner(PIN);
                    }

                    if (room != null)
                        return View(model);
                }
            }

            Error("Chat o podanym adresie nie istnieje!");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Details(Guid? roomId, string PIN)
        {
            ChatroomViewModel model = new ChatroomViewModel();
            room = null;
            if (roomId.HasValue)
            {
                room = manager.Get(roomId.Value) as ChatroomLite;

                if (room != null)
                {

                    model.Chatroom = room;
                    model.PIN = PIN;
                    model.roomId = roomId.Value;
                    model.IsOwner = room.IsOwner(PIN);
                }


                model.Users = room.GetUsers(model.PIN);

                Save();

                return View(model);
            }

            Error("Chat o podanym adresie nie istnieje!");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult ResetIncidents(Guid roomId, string PIN)
        {
            IChatroom room = manager.Get(roomId);
            if (room != null)
            {
                room.ClearIncidents(PIN);

                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json("Chat o podanym adresie nie istnieje!", JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public JsonResult Kick(Guid roomId, string PIN, Guid UserId)
        {
            IChatroom room = manager.Get(roomId);

            if (room != null)
            {
                room.Kick(PIN, UserId);

                Save();

                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json("Chat o podanym adresie nie istnieje!", JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Push(Guid roomId, string PIN, string Message)
        {
            Message = Regex.Replace(Message, "<", "&lt;");
            Message = Regex.Replace(Message, ">", "&gt;");
            Message = Regex.Replace(Message, "\"", "&quot;");
            Message = Regex.Replace(Message, "'", "&#39;");
            string accessMessage = string.Empty;

            ChatResult result = null;
            IChatroom room = manager.Get(roomId);
            if (room != null)
            {
                result = room.Push(PIN, Message);

                Save();
            }

            if (result == null && !string.IsNullOrEmpty(PIN)) accessMessage = "Twój slot został zamknięty!";
            if (room == null) accessMessage = "Chat został zamknięty!";

            return generateJsonChat(result, accessMessage, PIN);
        }

        [HttpPost]
        public JsonResult Pull(Guid roomId, string PIN)
        {
            string accessMessage = string.Empty;

            ChatResult result = null;
            IChatroom room = manager.Get(roomId);
            if (room != null)
            {
                result = room.Pull(PIN);
            }

            if (result == null && !string.IsNullOrEmpty(PIN)) accessMessage = "Twój slot został zamknięty!";
            if (room == null) accessMessage = "Chat został zamknięty!";

            return generateJsonChat(result, accessMessage, PIN);

        }


        [HttpPost]
        public JsonResult AddSlot(Guid roomId, string PIN)
        {
            IChatroom room = manager.Get(roomId);
            if (room != null)
            {
                ChatResult result = room.AddSlot(PIN);

                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json("Chat o podanym adresie nie istnieje!", JsonRequestBehavior.DenyGet);
            }
        }


        [HttpPost]
        public JsonResult ExtendTime(Guid roomId, string PIN)
        {
            IChatroom room = manager.Get(roomId);
            if (room != null)
            {
                ChatResult result = room.ExtendTime(PIN);

                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json("Chat o podanym adresie nie istnieje!", JsonRequestBehavior.DenyGet);
            }
        }


        private JsonResult generateJsonChat(ChatResult result, string accessMessage, string PIN)
        {
            if (result != null)
            {
                var jsonItem = new
                {
                    Messages = result.Messages.Select(p => new
                    {
                        text = string.Format("<span class='chatmessage_User'>{0}</span>: {1}", p.Author, p.Body),
                        type = p.MessageStatus.ToString()
                    }),
                    Access = true,
                    Users = result.Users,
                    Expiration = result.Expiration,
                    Incidents = result.Incidents
                };

                return Json(jsonItem, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(
                    new
                    {
                        Access = string.IsNullOrWhiteSpace(PIN) ? new Nullable<bool>() : false,
                        Messages = new List<string>(),
                        Users = new List<string>(),
                        AccessMessage = accessMessage,
                        Expiration = 0
                    }

                , JsonRequestBehavior.DenyGet);
            }
        }

    }
}
