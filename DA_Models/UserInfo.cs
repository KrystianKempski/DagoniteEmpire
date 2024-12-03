using DA_Models.CharacterModels;
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
        public CharacterDTO? SelectedCharacter { get; set; } = null;
        public int SelectedCharacterId { get; set; } = 0;
        public string? UserName { get; set; }
        public string? UserId { get; set; }
        public bool? IsAdminOrMG { get; set; }
        public bool? CharacterMG { get; set; }
        public bool? IsAuthenticated { get; set; }
        public string? Role { get; set; }
        public bool IsInited { get; set; } = false;
    }
}
