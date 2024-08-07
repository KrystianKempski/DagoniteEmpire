using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_DataAccess;
using DA_DataAccess.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DA_Business.Services
{
    public class UserService : IUserService
    {

        private readonly AuthenticationStateProvider _authState;
        private readonly UserInfo _userInfo;

        private readonly IDbContextFactory<ApplicationDbContext> _db;

        public UserService(AuthenticationStateProvider authState,IOptions<UserInfo> userInfo, IDbContextFactory<ApplicationDbContext> db)
        {
            _authState = authState;
            _userInfo = userInfo.Value;
            _db = db;
        }

        public async Task InitUserInfoAtStart()
        {
            try
            {
                if (_userInfo.UserId is null)
                {


                    var user = (await _authState.GetAuthenticationStateAsync()).User;
                    if (user is null)
                        return; // failed to load

                    _userInfo.IsAuthenticated = user.Identity?.IsAuthenticated;
                    _userInfo.UserId = user.Claims.Where(a => a.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
                    _userInfo.IsAdminOrMG = user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_GameMaster);
                    _userInfo.UserName = user.Identity?.Name;
                    if (user.IsInRole(SD.Role_HeroPlayer))
                    {
                        _userInfo.Role = SD.Role_HeroPlayer;
                    }else if (user.IsInRole(SD.Role_DukePlayer)){
                        _userInfo.Role = SD.Role_DukePlayer;
                    }
                    else if (user.IsInRole(SD.Role_Admin))
                    {
                        _userInfo.Role = SD.Role_Admin;
                    }
                    else if (user.IsInRole(SD.Role_GameMaster))
                    {
                        _userInfo.Role = SD.Role_GameMaster;
                    }
                    if (user.Identity?.IsAuthenticated is null)
                        return;
                    await GetUserInfo();
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        public async Task<UserInfo?> GetUserInfo()
        {
            if (_userInfo is not null) 
            { 

                if (_userInfo.IsUserUpdated == false)
                {
                    using var contex = await _db.CreateDbContextAsync();
                    var user = await contex.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == _userInfo.UserId);
                    if(user is not null)
                    {
                        _userInfo.SelectedCharacterId = user.SelectedCharacterId;
                        _userInfo.IsUserUpdated = true;
                    }
                }
            }
            else
            {
                await InitUserInfoAtStart();
            }
            return _userInfo;
        }
        public async Task SetSelectedCharId(int charId)
        {
            
            if (_userInfo is not null)
            {
                using var contex = await _db.CreateDbContextAsync();
                var user = await contex.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == _userInfo.UserId);
                if(user != null)
                {

                    user.SelectedCharacterId = charId;
                    var addedObj = contex.ApplicationUsers.Update(user);
                    await contex.SaveChangesAsync();
                    _userInfo.SelectedCharacterId = charId;
                }
            }

        }
    }
}
