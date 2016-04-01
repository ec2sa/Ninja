using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC2.ChatEngine
{
    public class Message
    {
        public int ID { get; private set; }
        public DateTime Date { get; private set; }
        public string Author { get; private set; }
        public string Body { get; private set; }
        public Status MessageStatus { get; set; }
		public HashFile File { get; set; }

        public Message(int id, string author, string body):this(id,author,body,Status.Message) { }


        public Message(int id, string author, string body, Status status)
        {
            this.ID = id;
            this.Author = author;
            this.Body = body;
            this.Date = DateTime.Now;
            this.MessageStatus = status;
        }

		public Message(int id, string author, string input, string FileType, string FileName)
		{
			this.ID = id;
			this.Author = "SYSTEM";
			this.File = new HashFile(input, FileName, FileType);
            this.Body = String.Format("{0} [SystemMessages_FileUploaded_1] {1} [SystemMessages_FileUploaded_2]", author, FileName);
			this.Date = DateTime.Now;
			this.MessageStatus = Status.File;
		}

        public enum Status
        {
            Info,
            Warning,
            Error,
            Message,
			File,
			Delete
        }
    }
}
