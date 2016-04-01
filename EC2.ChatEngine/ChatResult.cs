using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC2.ChatEngine
{
    public class ChatResult
    {
        public List<Message> Messages { get; set; }
        public List<UserInfo> Users { get; set; }
        public List<IncidentInfo> Incidents { get; set; }
        public int Expiration { get; set; }
        public string Pin { get; set; }
        public bool Terminate { get; set; }

        public ChatResult(IChatroom room, List<Message> messages, string pin)
        {
            Messages = messages;
            Pin = pin;
            if (room != null)
            {
                Users = room.Users(pin);
                Incidents = room.Incidents(pin);
                Expiration = calculateExpiration(room);
            }
        }

        public ChatResult(IChatroom room, List<Message> messages, string pin, bool shouldTerminate)
            : this(room, messages, pin)
        {
            Terminate = shouldTerminate;
        }

        public ChatResult(IChatroom room, string author, string message, string pin, Message.Status status = Message.Status.Info)
        {
            Messages = new List<Message>();
            Messages.Add(new Message(1, author, message, status));
            Pin = pin;
            if (room != null)
            {
                Users = room.Users(pin);
                Incidents = room.Incidents(pin);
                Expiration = calculateExpiration(room);
            }

        }

        public ChatResult(IChatroom room, string author, string message, string pin, bool shouldTerminate, Message.Status status)
            : this(room, author, message, pin, status)
        {
            Terminate = shouldTerminate;
        }

        private int calculateExpiration(IChatroom room)
        {
            return 100 * (int)((DateTime.Now - room.ExpirationStartsAt).Duration().TotalSeconds) / (room.TTLInMinutes * 60);
        }





    }
}
