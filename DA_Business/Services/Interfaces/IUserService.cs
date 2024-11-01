using DA_DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Services.Interfaces
{
    public interface IUserService
    {
        public Task<UserInfo?> GetUserInfo();
        public Task SetSelectedCharId(int charId);
    }
}
