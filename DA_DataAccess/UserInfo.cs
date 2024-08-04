using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess
{
    public class UserInfo
    {
        public int SelectedCharacterId { get; set; } = 0;
        public string? UserName { get; set; }

        public string? UserId { get; set; }

        public bool? IsAdminOrMG { get; set; }

        public bool? IsAuthenticated { get; set; }

        public string? Role { get; set; }

        public bool IsUserUpdated { get; set; }

    }
}
