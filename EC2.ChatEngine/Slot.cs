using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC2.ChatEngine
{
    public class Slot
    {
        public bool WasJoined { get; set; }
        
        public static int PinLength = 8;
        public Guid SlotID { get; set; }
        public string SlotPIN { get; private set; }
        public string Nick { get; set; }
        public int LastReadID { get; set; }
        public bool IsOwner { get; set; }
        
        public Slot(int? lastReadID)
        {
            SlotID = Guid.NewGuid();
            SlotPIN = Guid.NewGuid().ToString().Substring(0, PinLength);
            LastReadID = lastReadID ?? -1;
        }

        public Slot(string nick, bool isOwner, int? lastReadID):this(lastReadID)
        {
            Nick = nick;
            IsOwner = isOwner;
        }

        public bool GetSlot(string pin,string nick){
            if (SlotPIN.ToLower() != pin.ToLower())
                return false;
            Nick = nick;
            return true; 
        }
    }
}
