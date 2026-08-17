using DA_Common;
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

        /// <summary>Hidden "Try baron" / "Try Game Master" session.</summary>
        public bool IsDemoSession => SD.IsDemoUserName(UserName);

        /// <summary>
        /// Real Admin/GM who may list and open every character. Demo accounts never qualify.
        /// </summary>
        public bool HasGlobalCharacterAccess => SD.HasGlobalCharacterAccess(UserName, IsAdminOrMG == true);

        /// <summary>
        /// Demo sessions may only open the throwaway baron cloned for this browser session.
        /// Real GM/Admin may open any character. Regular players still need an ownership check.
        /// </summary>
        public bool CanAccessCharacter(int characterId)
        {
            if (HasGlobalCharacterAccess)
                return true;
            if (IsDemoSession)
                return characterId > 0 && characterId == SelectedCharacterId;
            return false;
        }
    }
}
