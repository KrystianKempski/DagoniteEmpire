using MudBlazor.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.Chat
{
    public class ChatMessage
    {
        public long Id { get; set; }
        public string FromUserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser? FromUser { get; set; }
        public virtual ApplicationUser? ToUser { get; set; }
        //public bool IsNotice => Message.StartsWith("[Notice]");

        public bool IsRead { get; set; } = false;
    }
}
