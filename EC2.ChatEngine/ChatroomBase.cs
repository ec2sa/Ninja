using System;
using System.Collections.Generic;
using System.Linq;

namespace EC2.ChatEngine
{
	public abstract class ChatroomBase : IChatroom
	{
		protected const int extensionTime = 4;

		protected List<IncidentInfo> incidents;

		public int TTLInMinutes { get; private set; }

		public DateTime CreatedAt { get; private set; }

		public DateTime ExpirationStartsAt { get; private set; }

		public Guid ChatroomID { get; private set; }

		public string ChatroomName { get; set; }

		protected List<Slot> slots { get; set; }

		public List<Message> Messages { get; set; }

		public int CurrentMessageID { get { return Messages.Count; } }

		#region Virtual methods

		public virtual ChatResult Init(string name, string nick)
		{
			TTLInMinutes = extensionTime;
			CreatedAt = DateTime.Now;
			ExpirationStartsAt = CreatedAt;
			ChatroomID = Guid.NewGuid();
			ChatroomName = name;
			Messages = new List<Message>();
			slots = new List<Slot>();
			var slot = new Slot(nick, true, -1);
			slots.Add(slot);
			incidents = new List<IncidentInfo>();
			var messages = new List<Message>();
            Messages.Add(new Message(CurrentMessageID, "SYSTEM", string.Format("{0} [SystemMessages_CreatedRoom] {1}", nick, name), Message.Status.Info));
            messages.Add(new Message(CurrentMessageID, "SYSTEM", "[SystemMessages_OwnerSlotAdded].", Message.Status.Info));
            messages.Add(new Message(CurrentMessageID, "SYSTEM", "[SystemMessages_OwnerNick]: " + slot.Nick, Message.Status.Info));

			return new ChatResult(this, messages, slot.SlotPIN);
		}

		public virtual ChatResult Join(string nick, string pin)
		{
			var messages = new List<Message>();
			var slot = slots.FirstOrDefault(s => s.SlotPIN.ToLower() == pin.ToLower());
			if (slot == null)
			{
                messages.Add(new Message(1, nick, "[SystemMessages_CannotJoin]", Message.Status.Error));
                incidents.Add(new IncidentInfo() { Description = "[SystemMessages_NoPinFound]" });
				return new ChatResult(this, messages, pin, true);
			}

			if (slot.WasJoined)
			{
                messages.Add(new Message(1, nick, "[SystemMessages_CannotJoin]", Message.Status.Error));
                incidents.Add(new IncidentInfo() { Description = "[SystemMessages_PinUsedBefore]" });
				return new ChatResult(this, messages, pin, true);
			}

			slot.Nick = nick;
			slot.WasJoined = true;
			Messages.Add(new Message(CurrentMessageID, "SYSTEM", string.Format("{0} przyłączył się do rozmowy.", nick), Message.Status.Warning));
            Messages.Add(new Message(CurrentMessageID, "SYSTEM", string.Format("[SystemMessages_Welcome] \"{0}\", {1}", this.ChatroomName, nick), Message.Status.Info));
			return new ChatResult(this, messages, pin);
		}

		public virtual List<UserInfo> GetUsers(string pin)
		{
			if (!IsOwner(pin))
				return new List<UserInfo>();
			return slots.Select(s => new UserInfo() { Nick = s.Nick, SlotID = s.SlotID, Pin = s.SlotPIN }).ToList();
		}

		public virtual List<UserInfo> Users(string pin)
		{
			if (IsOwner(pin))
				return slots.OrderBy(s => s.Nick).Select(s => new UserInfo() { Nick = s.Nick, Pin = s.SlotPIN, SlotID = s.SlotID }).ToList();
			else
				return slots.OrderBy(s => s.Nick).Select(s => new UserInfo() { Nick = s.Nick }).ToList();
		}

		public virtual List<IncidentInfo> Incidents(string pin)
		{
			//return incidents;
			if (IsOwner(pin))
				return incidents;
			else
				return new List<IncidentInfo>();
		}

		public virtual Slot this[Guid guid]
		{
			get
			{
				return slots.FirstOrDefault(s => s.SlotID == guid);
			}
		}

		public virtual Slot this[string pin]
		{
			get
			{
				return slots.FirstOrDefault(s => s.SlotPIN.ToLower() == pin.ToLower());
			}
		}

		public virtual ChatResult AddSlot(string pin)
		{
			if (!IsOwner(pin))
                return new ChatResult(this, "SYSTEM", "[SystemMessages_NotAuthorized]", pin, Message.Status.Error);

			var slot = new Slot(CurrentMessageID);
			slots.Add(slot);
			var messages = new List<Message>();
            messages.Add(new Message(CurrentMessageID, "SYSTEM", "[SystemMessages_NewSlotAdded]" + slot.SlotPIN, Message.Status.Warning));
			return new ChatResult(this, messages, pin);
		}

		public virtual bool IsOwner(string pin)
		{
			return slots.SingleOrDefault(s => pin != null && s.SlotPIN.ToLower() == pin.ToLower() && s.IsOwner) != null;
		}

		public virtual ChatResult Pull(string pin)
		{
			var slot = this[pin];
			if (slot == null) return null;
			var result = Messages.Where(m => m.ID > slot.LastReadID).ToList();
			slot.LastReadID = CurrentMessageID - 1;

			return new ChatResult(this, result, pin);
		}

		public virtual ChatResult Push(string pin, string message)
		{
			var slot = this[pin];
			if (slot == null)
                return new ChatResult(this, "SYSTEM", "[SystemMessages_SlotNotFound]", pin, true, Message.Status.Error);

			addMessage(slot, message);

			var result = Messages.Where(m => m.ID > slot.LastReadID).ToList();
			slot.LastReadID = CurrentMessageID - 1;

			return new ChatResult(this, result, pin);
		}

		/// <summary>
		/// Metoda dodająca plik do serwera
		/// </summary>
		/// <param name="pin">PIN osoby wywołującej akcje</param>
		/// <param name="input">Zawartość pliku</param>
		/// <param name="FileType">Typ pliku</param>
		/// <param name="FileName">Nazwa pliku</param>
		/// <returns>Zwraca rezultat wywołanej metody</returns>
		public virtual ChatResult Push(string pin, string input, string FileType, string FileName)
		{
			var slot = this[pin];
			if (slot == null)
                return new ChatResult(this, "SYSTEM", "[SystemMessages_SlotNotFound]", pin, true, Message.Status.Error);

			addMessage(slot, input, FileType, FileName);

			var result = Messages.Where(m => m.ID > slot.LastReadID).ToList();
			slot.LastReadID = CurrentMessageID - 1;

			return new ChatResult(this, result, pin);
		}

		protected virtual void addMessage(Slot slot, string message)
		{
			Messages.Add(new Message(CurrentMessageID, slot.Nick, message, Message.Status.Message));
		}

		/// <summary>
		/// Metoda dodająca plik do listy wiadomości
		/// </summary>
		/// <param name="slot">Slot wrzucanej wiadomości</param>
		/// <param name="input">Zawartość pliku</param>
		/// <param name="FileType">Typ pliku</param>
		/// <param name="FileName">Nazwa pliku</param>
		protected virtual void addMessage(Slot slot, string input, string FileType, string FileName)
		{
			Messages.Add(new Message(CurrentMessageID, slot.Nick, input, FileType, FileName));
		}

		/// <summary>
		/// Metoda dodająca wiadomość do listy wiadomości
		/// </summary>
		/// <param name="slot">Slot wrzucanej wiadomości</param>
		/// <param name="message">Wiadomość</param>
		/// <param name="type">Typ wiadomości</param>
		protected virtual void addMessage(Slot slot, string message, Message.Status type)
		{
			Messages.Add(new Message(CurrentMessageID, slot.Nick, message, type));
		}

		public virtual ChatResult Kick(string pin, Guid slotID)
		{
			if (!IsOwner(pin))
                return new ChatResult(this, "SYSTEM", "[SystemMessages_NotAuthorized]", pin, Message.Status.Error);

			var slot = this[slotID];
			if (slot == null)
                return new ChatResult(this, "SYSTEM", "[SystemMessages_SlotNotFound]", pin, Message.Status.Error);
			var nick = slot.Nick;
			slots.Remove(slot);
            return new ChatResult(this, "SYSTEM", string.Format("{0} [SystemMessages_WasKicked].", nick), pin, Message.Status.Warning);
		}

		public virtual ChatResult Leave(string pin)
		{
			var slot = this[pin];
			if (slot == null)
                return new ChatResult(this, "SYSTEM", "[SystemMessages_SlotNotFound]", pin, true, Message.Status.Error);
			slots.Remove(slot);
            return new ChatResult(this, "SYSTEM", string.Format("{0} [SystemMessages_LeftBuilding]", slot.Nick), null, true, Message.Status.Warning);
		}

		public virtual ChatResult ExtendTime(string pin)
		{
			if (!IsOwner(pin))
                return new ChatResult(this, "SYSTEM", "[SystemMessages_NotAuthorized]", pin, Message.Status.Error);
			ExpirationStartsAt = DateTime.Now;
            return new ChatResult(this, "SYSTEM", "[SystemMessages_RoomExistanceExtended]", pin, Message.Status.Info);
		}

		public virtual ChatResult ClearIncidents(string pin)
		{
			if (!IsOwner(pin))
                return new ChatResult(this, "SYSTEM", "[SystemMessages_NotAuthorized]", pin, Message.Status.Error);
			incidents.Clear();
            return new ChatResult(this, "SYSTEM", "[SystemMessages_IncidentsClear]", pin, Message.Status.Info);
		}

		public virtual bool HasExpired
		{
			get
			{
				return this.ExpirationStartsAt.AddMinutes(this.TTLInMinutes) <= DateTime.Now;
			}
		}

		/// <summary>
		/// Metoda pobierająca plik z serwera w postaci HashFile
		/// </summary>
		/// <param name="pin">PIN osoby wywołującej akcje</param>
		/// <returns>Zwraca plik w postaci HashFile</returns>
		public virtual HashFile GetFile(string pin, int ID)
		{
			var slot = this[pin];
			if (slot == null) return null;
			var result = Messages.FirstOrDefault(m => m.MessageStatus == Message.Status.File && m.ID == ID);
			if (result != null)
				return result.File;
			return null;
		}

		/// <summary>
		/// Metoda usuwa plik z serwera
		/// </summary>
		/// <param name="pin">PIN osoby wywołującej akcje</param>
		/// <returns>Zwraca rezultat wywołanej metody</returns>
		public virtual ChatResult DelFile(string pin, int ID)
		{
			var slot = this[pin];
			if (slot == null) return null;

			var del = Messages.FirstOrDefault(m => m.MessageStatus == Message.Status.File && m.ID == ID);
			if (del != null)
			{
				Messages.Remove(del);
                addMessage(slot, string.Format("[SystemMessages_FileDeleted_1] {0} [SystemMessages_FileDeleted_2] {1}", del.File.Name, slot.Nick), Message.Status.Delete);
				slots.ForEach(it => it.LastReadID--);
			}
			else
			{
                addMessage(slot, string.Format("{0} [SystemMessages_DeleteFileNotExist]", slot.Nick));
			}
			var result = Messages.Where(m => m.ID > slot.LastReadID).ToList();

			slot.LastReadID = CurrentMessageID - 1;

			return new ChatResult(this, result, pin);
		}

		#endregion Virtual methods
	}
}