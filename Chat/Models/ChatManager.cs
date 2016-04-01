using EC2.ChatEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EC2.Chat.Models
{
    public class ChatManager<TChatroom>
        where TChatroom : IChatroom, new()
    {
        protected List<TChatroom> chatrooms { get; set; }

        public ChatManager()
        {
            chatrooms = new List<TChatroom>();
        }

        public bool Exists(Guid roomID)
        {
            return (chatrooms.FirstOrDefault(r => r.ChatroomID == roomID) != null);
        }

        public TChatroom Add(string roomName, string adminNick, out ChatResult result)
        {
            var room = new TChatroom();
            result = room.Init(roomName, adminNick);
            chatrooms.Add(room);
            return room;
        }

        public IChatroom Get(Guid roomID)
        {
            var room = chatrooms.SingleOrDefault(r => r.ChatroomID == roomID);

            if (room != null && room.HasExpired)
            {
                chatrooms.Remove(room);
                return null;
            }
            return room;
        }

        public ChatResult Delete(Guid roomID, string pin)
        {
            var room = chatrooms.SingleOrDefault(r => r.ChatroomID == roomID);
            if (room != null)
            {
                if (room.IsOwner(pin))
                {
                    chatrooms.Remove(room);
                    return new ChatResult(room, "SYSTEM", "Chatroom has been closed by owner", pin);
                }
            }
            return new ChatResult(room, "SYSTEM", "Not authorized or chatroom doesn't exists", pin);
        }

    }
}