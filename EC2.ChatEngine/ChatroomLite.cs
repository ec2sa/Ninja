using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC2.ChatEngine
{
    public class ChatroomLite : ChatroomBase
    {
        public ChatResult Join(string nick)
        {
            var slot = new Slot(CurrentMessageID);
            slots.Add(slot);
            var messages = new List<Message>();
            
            slot.Nick = nick;
            slot.WasJoined = true;
            Messages.Add(new Message(CurrentMessageID, "SYSTEM", string.Format("{0} dołączyłdo rozmowy.", nick),Message.Status.Warning));
            Messages.Add(new Message(CurrentMessageID, "SYSTEM", string.Format("Witaj w pokoju \"{0}\", {1}", this.ChatroomName, nick), Message.Status.Info));
            return new ChatResult(this, messages, slot.SlotPIN);
        }

        public List<UserInfo> GetUsers(string pin)
        {
            if (!IsOwner(pin))
                return new List<UserInfo>();
            return slots.Select(s => new UserInfo() { Nick = s.Nick, SlotID = s.SlotID}).ToList();
        }


        public override ChatResult Join(string nick, string pin)
        {
            return Join(nick);
        }

     
    }
}
