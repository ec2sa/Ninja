using EC2.Chat.Helpers;
using EC2.Chat.Models;
using EC2.ChatEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EC2.Chat.Controllers
{
    public class ChatroomController : BaseController<Chatroom>
	{
		private IChatroom room = null;

        [CultureAttribute]
        public ActionResult Create()
		{
			ChatroomViewModel model = new ChatroomViewModel();
			return View(model);
		}

		[HttpPost]
		public ActionResult Create(string Name, string Nick)
		{
			if (string.IsNullOrWhiteSpace(Name)) Name = Resources.LocalizedText.SystemMessages_Anonymous;
            if (string.IsNullOrWhiteSpace(Nick)) Nick = Resources.LocalizedText.SystemMessages_Anonymous;

			ChatResult result = null;
			room = manager.Add(Name, Nick, out result);
			TempData["PIN"] = result.Pin;

			Save();
			return RedirectToAction("Details", new { roomId = room.ChatroomID });
		}

		[HttpPost]
        [CultureAttribute]
		public ActionResult Join(Guid? roomId, string PIN, string Nick)
		{
            if (string.IsNullOrWhiteSpace(Nick)) Nick = Resources.LocalizedText.SystemMessages_Anonymous;

			if (roomId.HasValue)
			{
				var room = manager.Get(roomId.Value);

				if (room != null)
				{
					ChatResult result = room.Join(Nick, PIN);
					if (result.Terminate == true)
					{
                        Error(Resources.LocalizedText.SystemMessages_WrongOrUsedPIN);
						return RedirectToAction("Index", "Home");
					}

					Save();

					TempData["PIN"] = PIN;

					return RedirectToAction("Details", new { roomId = room.ChatroomID });
				}
				else
				{
                    Error(Resources.LocalizedText.SystemMessages_ChatroomNotExist);
					return RedirectToAction("Index", "Home");
				}
			}

            Error(Resources.LocalizedText.SystemMessages_ChatroomNotExist);
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

                Information(Resources.LocalizedText.SystemMessages_LoggedOffSuccessfully);

				return RedirectToAction("Index", "Home");
			}

            Error(Resources.LocalizedText.SystemMessages_UserPinNotExist);
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public ActionResult DeleteRoom(Guid roomId, string PIN)
		{
			ChatResult result = manager.Delete(roomId, PIN);

			Save();

			Error(Resources.LocalizedText.ChatWindow_Messages_OnClose);
			return RedirectToAction("Index", "Home");
		}

        [CultureAttribute]
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
						var mFile = ((Chatroom)room).Messages.FirstOrDefault(it => it.MessageStatus == Message.Status.File);
						model.isFile = mFile != null;
						model.File = model.isFile ? mFile.File.Name : null;
					}

					if (room != null)
						return View(model);
				}
			}

            Error(Resources.LocalizedText.SystemMessages_ChatroomNotExist);
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
        [CultureAttribute]
		public ActionResult Details(Guid? roomId, string PIN)
		{
			ChatroomViewModel model = new ChatroomViewModel();
			room = null;
			if (roomId.HasValue)
			{
				room = manager.Get(roomId.Value);

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

            Error(Resources.LocalizedText.SystemMessages_ChatroomNotExist);
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
                return Json(Resources.LocalizedText.SystemMessages_ChatroomNotExist, JsonRequestBehavior.DenyGet);
			}
		}

		[HttpPost]
		public JsonResult Kick(Guid roomId, string PIN, Guid UserId)
		{
			var room = manager.Get(roomId);

			if (room != null)
			{
				var chat = room.Kick(PIN, UserId);
				Save();
				return generateJsonChat(chat, null, null);
			}
            return Json(Resources.LocalizedText.SystemMessages_ChatroomNotExist, JsonRequestBehavior.DenyGet);
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

            if (result == null && !string.IsNullOrEmpty(PIN)) accessMessage = Resources.LocalizedText.SystemMessages_YoursSlotClosed;
            if (room == null) accessMessage = Resources.LocalizedText.SystemMessages_ChatClosed;

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

            if (result == null && !string.IsNullOrEmpty(PIN)) accessMessage = Resources.LocalizedText.SystemMessages_YoursSlotClosed;
            if (room == null) accessMessage = Resources.LocalizedText.SystemMessages_ChatClosed;
            
			return generateJsonChat(result, accessMessage, PIN);
		}

		[HttpPost]
		public JsonResult AddSlot(Guid roomId, string PIN)
		{
			var chatroom = manager.Get(roomId);
            if (chatroom == null) return Json(Resources.LocalizedText.SystemMessages_ChatroomNotExist, JsonRequestBehavior.DenyGet);
			var result = chatroom.AddSlot(PIN);
			return generateJsonChat(result, null, null); //Json(null, JsonRequestBehavior.DenyGet);
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
                return Json(Resources.LocalizedText.SystemMessages_ChatroomNotExist, JsonRequestBehavior.DenyGet);
			}
		}

		/// <summary>
		/// Akcja odpowiadająca za upload pliku na serwer
		/// </summary>
		/// <param name="roomId">ID chatu</param>
		/// <param name="PIN">PIN osoby wywołującej akcje</param>
		/// <param name="FileStream">Plik do uploadu</param>
		/// <returns>Zwraca informacje o stanie akcji</returns>
		[HttpPost]
		public JsonResult UploadFile(Guid roomId, string PIN, HttpPostedFileBase FileStream)
		{
			if (FileStream != null && FileStream.ContentLength > 0 && FileStream.FileName != null)
			{
				string accessMessage = string.Empty;

				ChatResult result = null;
				IChatroom room = manager.Get(roomId);
				if (room != null && FileStream.ContentLength > 0 && FileStream.ContentLength < 8192 * 1024)
				{
					using (var mem = new System.IO.MemoryStream())
					{
						var serial = new System.Web.Script.Serialization.JavaScriptSerializer {MaxJsonLength = 8192*1024};
						FileStream.InputStream.CopyTo(mem);
						var contents = mem.ToArray();
						var a = serial.Serialize(contents);
						var name = FileStream.FileName.Contains("\\") ? 
							FileStream.FileName.Split('\\').Last() : 
							FileStream.FileName;
						result = room.Push(PIN, a, FileStream.ContentType, name);
					}
					Save();
				}
				else
                    accessMessage = Resources.LocalizedText.SystemMessages_FileToBig;
                if (room == null) accessMessage = Resources.LocalizedText.SystemMessages_ChatClosed;

				return generateJsonChat(result, accessMessage, PIN);
			}
			return null;
		}

		/// <summary>
		/// Usunięcie pliku z serwera
		/// </summary>
		/// <param name="roomId">ID chatu</param>
		/// <param name="PIN">PIN osoby wywołującej akcje</param>
		/// <returns>Zwraca informacje o stanie akcji</returns>
		[HttpPost]
		public JsonResult DeleteFile(Guid roomId, string PIN, int ID)
		{
			string accessMessage = string.Empty;

			ChatResult result = null;
			IChatroom room = manager.Get(roomId);
			if (room != null)
			{
				result = room.DelFile(PIN, ID);

				Save();
			}

            if (result == null && !string.IsNullOrEmpty(PIN)) accessMessage = Resources.LocalizedText.SystemMessages_YoursSlotClosed;
            if (room == null) accessMessage = Resources.LocalizedText.SystemMessages_ChatClosed;

			return generateJsonChat(result, accessMessage, PIN);
		}

		/// <summary>
		/// Pobieranie pliku z serwera
		/// </summary>
		/// <param name="roomId">ID chatu</param>
		/// <param name="PIN">PIN osoby wywołującej akcje</param>
		/// <returns>Zwraca informacje o stanie akcji</returns>
		public FileResult DownFile(Guid roomId, string PIN, int ID)
		{
			IChatroom room = manager.Get(roomId);
			if (room != null)
			{
				var file = room.GetFile(PIN, ID);

				var serial = new System.Web.Script.Serialization.JavaScriptSerializer();
				serial.MaxJsonLength = 8192 * 1024;
				try
				{
					var baContent = serial.Deserialize(file.Content, typeof(byte[]));
					var content = new MemoryStream((byte[])baContent);
					FileStreamResult ret = base.File(content, file.Type, file.Name);
					return ret;
				}
				catch (Exception e)
				{
					return null;
				}
			}
			return null;
		}

        private string PickColor(string name)
        {
            if (name == "SYSTEM") return "#EEFFCC";
            
            string[] colors = new String[]{
                "#FF0000", "#33FF33", "#6666FF", "#FF66FF", "#99FFCC", "#CC99FF", "#CCFF99", "#FFFF00"
            };
            char[] chars = name.ToCharArray();
            int sum = 0;
            
            foreach(char c in chars)
            {
                sum += (int)c;
            }

            int index = (sum % colors.Length);
            return colors[index];
        }

		private JsonResult generateJsonChat(ChatResult result, string accessMessage, string PIN)
		{
			if (result != null)
			{
				var file = result.Messages.FirstOrDefault(it => it.MessageStatus == Message.Status.File || Message.Status.Delete == it.MessageStatus);
                //List<HashFile> files = new List<HashFile>();
                //foreach (var item in result.Messages.Where(it => it.MessageStatus == Message.Status.File || Message.Status.Delete == it.MessageStatus))
                //{
                //    if (item.File != null && item.MessageStatus == Message.Status.File)
                //        files.Add(item.File);
                //}
                if (file != null && file.MessageStatus == Message.Status.Delete)
                {
                    bool BreakPoint = true;
                }

				var jsonItem = new
				{
					Messages = result.Messages.Select(p => new
					{
						text = string.Format("<span class='chatmessage_User' style='color:{2};'>{0}</span>: {1}", p.Author, p.Body, PickColor(p.Author)),
						type = p.MessageStatus.ToString(),
					}),
					Access = true,
					Users = result.Users,
					Expiration = result.Expiration,
					Incidents = result.Incidents,
					File = file != null && file.MessageStatus == Message.Status.File ? file.File.Name : null,
                    FileID = file != null && (file.MessageStatus == Message.Status.File || file.MessageStatus == Message.Status.Delete) ? file.ID : 0,
					Delete = file != null && file.MessageStatus == Message.Status.Delete
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