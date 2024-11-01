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
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DA_Business.Services
{
    public class UserService : IUserService
    {

        private readonly AuthenticationStateProvider _authState;
        private readonly UserInfo _userInfo;
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly IDbContextFactory<ApplicationDbContext> _db;

        public UserService(AuthenticationStateProvider authState, IOptions<UserInfo> userInfo, IDbContextFactory<ApplicationDbContext> db, ProtectedSessionStorage sessionStorage)
        {
            _authState = authState;
            _userInfo = userInfo.Value;
            _db = db;
            _sessionStorage = sessionStorage;
        }

        public async Task InitUserInfoAtStart()
        {
            try
            {


                if (_userInfo.InitUser)
                {
                    var user = (await _authState.GetAuthenticationStateAsync()).User;
                    if (user is null)
                        return; // failed to load

                    _userInfo.IsAuthenticated = user.Identity?.IsAuthenticated;
                    _userInfo.UserId = user.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
                    _userInfo.IsAdminOrMG = user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_GameMaster);
                    _userInfo.UserName = user.Identity?.Name;
                    if (user.IsInRole(SD.Role_HeroPlayer))
                    {
                        _userInfo.Role = SD.Role_HeroPlayer;
                    }
                    else if (user.IsInRole(SD.Role_DukePlayer))
                    {
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
                    _userInfo.InitUser = false;

                    await _sessionStorage.SetAsync("userinfo", _userInfo);
                   // await GetUserInfo();
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
        public void OrderToClearUserInfo()
        {
            try
            {
                _userInfo.ResetUser = true;
            }
            catch (Exception ex)
            {
                ;
            }
        }
        public void OrderToInitUserInfo()
        {
            try
            {
                _userInfo.InitUser = true;
            }
            catch (Exception ex)
            {
                ;
            }
        }
        public async Task ClearUserInfo()
        {
            try
            {
                UserInfo userInfo = new();
                _userInfo.IsAdminOrMG = userInfo.IsAdminOrMG;
                _userInfo.SelectedCharacterId = userInfo.SelectedCharacterId;
                _userInfo.UserName = userInfo.UserName;
                _userInfo.UserId = userInfo.UserId;
                _userInfo.IsAuthenticated = userInfo.IsAuthenticated;
                _userInfo.Role = userInfo.Role;
                _userInfo.IsUserUpdated = userInfo.IsUserUpdated;
                _userInfo.ResetUser = false;
                await _sessionStorage.SetAsync("userinfo", _userInfo);
            }
            catch (Exception ex)
            {
                ;
            }
        }
        public async Task<UserInfo?> GetUserInfo()
        {
            try
            {
                if (_userInfo is not null)
                {
                    if (_userInfo.ResetUser)
                    {
                        await ClearUserInfo();

                    }
                    if (_userInfo.InitUser)
                    {
                        await InitUserInfoAtStart();
                    }

                    //if (_userInfo.IsUserUpdated == false)
                    //{                       
                        var res = await _sessionStorage.GetAsync<UserInfo>("userinfo");
                        if (res.Value is null) return _userInfo;
                        _userInfo.IsAdminOrMG = res.Value.IsAdminOrMG;
                        _userInfo.UserName = res.Value.UserName;
                        _userInfo.UserId = res.Value.UserId;
                        _userInfo.IsAuthenticated = res.Value.IsAuthenticated;
                        _userInfo.Role = res.Value.Role;
                        using var contex = await _db.CreateDbContextAsync();
                        var user = await contex.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == _userInfo.UserName);
                        if (user is not null)
                        {
                            _userInfo.SelectedCharacterId = user.SelectedCharacterId;
                            _userInfo.IsUserUpdated = true;
                        }
                        return _userInfo;
                   // }
                }
                else
                {
                    await InitUserInfoAtStart();
                }
                return _userInfo;
            }
            catch (Exception ex)
            {
                return _userInfo;
            }
        }
        public async Task SetSelectedCharId(int charId)
        {

            if (_userInfo is not null)
            {
                using var contex = await _db.CreateDbContextAsync();
                var user = await contex.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == _userInfo.UserName);
                if (user != null)
                {

                    user.SelectedCharacterId = charId;
                    var addedObj = contex.ApplicationUsers.Update(user);
                    await contex.SaveChangesAsync();
                    _userInfo.SelectedCharacterId = charId;
                    await _sessionStorage.SetAsync("userinfo", _userInfo);
                }

            }

        }
    }
}
