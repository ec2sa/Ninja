using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EC2.ChatEngine;

namespace EC2.Chat.Models
{
	public class ChatroomViewModel
	{

		public ChatroomViewModel()
		{

		}
		public Guid roomId { get; set; }
		public IChatroom Chatroom { get; set; }

		public bool IsOwner { get; set; }

		public List<UserInfo> Users { get; set; }

		[Display(Name = "PIN")]
		[Required]
		public string PIN { get; set; }

		public bool isFile { get; set; }

		public string File { get; set; }

		public string FileId
		{
			get
			{
				const string s =@"[! #$%&'\(\)\*\+,\.\/:;<=>\?\@\[\\\]\^`\{\|\}~]";
				return System.Text.RegularExpressions.Regex.Replace(File, s,"");
			}
		}
	}
}