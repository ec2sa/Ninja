using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC2.ChatEngine
{
    public interface IChatroom
    {
        Guid ChatroomID { get; }
        bool HasExpired { get; }
        int TTLInMinutes { get; }
        DateTime ExpirationStartsAt { get; }
        string ChatroomName { get; set; }


        ChatResult Init(string name, string nick);
        ChatResult Push(string pin, string message);
		ChatResult Push(string pin, string input, string FileType, string FileName);
        ChatResult Pull(string pin);
        ChatResult Leave(string pin);
        ChatResult Kick(string pin, Guid slotID);
        ChatResult ExtendTime(string pin);
        ChatResult ClearIncidents(string pin);
        ChatResult AddSlot(string pin);
        ChatResult Join(string nick, string pin);
        List<UserInfo> GetUsers(string pin);
		HashFile GetFile(string pin, int ID);
		ChatResult DelFile(string pin, int ID);

        List<IncidentInfo> Incidents(string pin);
        List<UserInfo> Users(string pin);
        bool IsOwner(string pin);
    }
}
