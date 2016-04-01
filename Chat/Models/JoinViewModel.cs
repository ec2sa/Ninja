using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EC2.Chat.Models
{
    public class JoinViewModel
    {
        [Required]
        public Guid ID { get; set; }
        
        [Required]
        public string PIN { get; set; }
    }
}