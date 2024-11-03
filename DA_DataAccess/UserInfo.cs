using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess
{
    public class UserInfo
    {
        public UserInfo() { }
        public bool InitUser { get; set; } = false;
        public bool ResetUser { get; set; } = false;
        public int SelectedCharacterId { get; set; } = 0;
        public string? UserName { get; set; }
        public string? UserId { get; set; }
        public bool? IsAdminOrMG { get; set; }
        public bool? IsAuthenticated { get; set; }
        public string? Role { get; set; }
    }
}
