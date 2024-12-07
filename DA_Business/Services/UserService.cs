using System.Diagnostics.Metrics;
using System.Security.Claims;
using AutoMapper;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_DataAccess;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DA_Business.Services
{
    public class UserService : IUserService
    {

        private readonly AuthenticationStateProvider _authState;
        private readonly UserInfo _userInfo;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;
        private readonly IJSRuntime _jsRuntime;
        private readonly ProtectedLocalStorage _protectedLocalStorage;


        public UserService(AuthenticationStateProvider authState, IOptions<UserInfo> userInfo, IDbContextFactory<ApplicationDbContext> db, IMapper mapper, IJSRuntime jsRuntime, ProtectedLocalStorage protectedLocalStorage)
        {
            _authState = authState;
            _userInfo = userInfo.Value;
            _db = db;
            _mapper = mapper;
            _jsRuntime = jsRuntime;
            _protectedLocalStorage = protectedLocalStorage;
        }

        public async Task<bool> IsAuthenticated()
        {
            if (_userInfo is null) throw new Exception("too fast authentication");

            var user = (await _authState.GetAuthenticationStateAsync()).User;
            if (user is null)
                throw new Exception("too fast authentication");

            return user.Identity?.IsAuthenticated == true;
        }
        //public async Task SelectCharacter()
        //{



        //    await _protectedLocalStorage.SetAsync("localdata", data);
        //}

        public async Task InitUserInfo()
        {
            if (_userInfo is null) throw new Exception("too fast authentication");

            var user = (await _authState.GetAuthenticationStateAsync()).User;
            if (user is null)
                throw new Exception("too fast authentication");

            if (user.Identity?.IsAuthenticated == false)
                return;
            _userInfo.IsAuthenticated = user.Identity?.IsAuthenticated;
            await _protectedLocalStorage.SetAsync("IsAuthenticated", _userInfo.IsAuthenticated);
            _userInfo.UserId = user.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
            await _protectedLocalStorage.SetAsync("UserId", _userInfo.UserId);
            _userInfo.IsAdminOrMG = user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_GameMaster);
            await _protectedLocalStorage.SetAsync("IsAdminOrMG", _userInfo.IsAdminOrMG);
            _userInfo.UserName = user.Identity?.Name;
            await _protectedLocalStorage.SetAsync("UserName", _userInfo.UserName);
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
            await _protectedLocalStorage.SetAsync("Role", _userInfo.Role);
            if (user.Identity?.IsAuthenticated is null)
                throw new Exception("Not authenticated");

            using var contex = await _db.CreateDbContextAsync();

            Character? obj = null;
            if (_userInfo.IsAdminOrMG == true)
            {
                obj = await contex.Characters.FirstOrDefaultAsync(u => u.NPCName == SD.GameMaster_NPCName);
              
                _userInfo.CharacterMG = true;
            }
            else
            {
                obj = await contex.Characters.FirstOrDefaultAsync(u => u.UserName == _userInfo.UserName);
                _userInfo.CharacterMG = false;
            }
            if (obj is not null)
            {
                _userInfo.SelectedCharacter = _mapper.Map<Character, CharacterDTO>(obj);
                _userInfo.SelectedCharacterId = _userInfo.SelectedCharacter.Id;
                await _protectedLocalStorage.SetAsync("SelectedCharacterId", _userInfo.SelectedCharacterId);
            }
            await _protectedLocalStorage.SetAsync("CharacterMG", _userInfo.CharacterMG);
            _userInfo.IsInited = true;
            await _protectedLocalStorage.SetAsync("IsInited", _userInfo.IsInited);
        }

        public async Task<UserInfo?> GetUserInfo()
        {
            try
            {

                if (_userInfo == null) return null;

                var resultIsInited = await _protectedLocalStorage.GetAsync<bool>("IsInited");
                _userInfo.IsInited = resultIsInited.Success ? resultIsInited.Value : false;
                if (_userInfo.IsInited == false)
                    await InitUserInfo();

                var resultIsAuthenticated = await _protectedLocalStorage.GetAsync<bool?>("IsAuthenticated");
                _userInfo.IsAuthenticated = resultIsAuthenticated.Success ? resultIsAuthenticated.Value : null;

                var resultUserId = await _protectedLocalStorage.GetAsync<string?>("UserId");
                _userInfo.UserId = resultUserId.Success ? resultUserId.Value : null;

                var resultIsAdminOrMG = await _protectedLocalStorage.GetAsync<bool?>("IsAdminOrMG");
                _userInfo.IsAdminOrMG = resultIsAdminOrMG.Success ? resultIsAdminOrMG.Value : null;

                var resultUserName = await _protectedLocalStorage.GetAsync<string?>("UserName");
                _userInfo.UserName = resultUserName.Success ? resultUserName.Value : null;

                var resultRole = await _protectedLocalStorage.GetAsync<string?>("Role");
                _userInfo.Role = resultRole.Success ? resultRole.Value : null;

                var resultSelectedCharacterId = await _protectedLocalStorage.GetAsync<int>("SelectedCharacterId");
                _userInfo.SelectedCharacterId = resultSelectedCharacterId.Success ? resultSelectedCharacterId.Value : 0;
                if(_userInfo.SelectedCharacterId == 0)
                {
                    _userInfo.SelectedCharacter = null;
                }
                else
                {
                    if (_userInfo.SelectedCharacter is null || _userInfo.SelectedCharacter.Id != _userInfo.SelectedCharacterId)
                    {
                        using var contex = await _db.CreateDbContextAsync();
                        var obj = await contex.Characters.FirstOrDefaultAsync(u => u.Id == _userInfo.SelectedCharacterId);
                        if (obj == null)
                        {
                            throw new Exception($"Error! No character found");
                        }
                        _userInfo.SelectedCharacter = _mapper.Map<Character, CharacterDTO>(obj);
                    }
                }
                var resultCharacterMG = await _protectedLocalStorage.GetAsync<bool?>("CharacterMG");
                _userInfo.CharacterMG = resultCharacterMG.Success ? resultCharacterMG.Value : null;

                return _userInfo;
                
            }
            catch (Exception ex)
            {
                throw new Exception($"error: {ex.Message}");
            }
        }


        public async Task SetSelectedCharId(int charId)
        {
            try
            {
                if (_userInfo is not null && charId != _userInfo.SelectedCharacterId)
                {
                    using var contex = await _db.CreateDbContextAsync();

                    Character? obj = null;
                    if (charId == -1)
                    {

                        obj = await contex.Characters.FirstOrDefaultAsync(u => u.NPCName == SD.GameMaster_NPCName);
                        _userInfo.CharacterMG = true;
                    }
                    else
                    {
                        obj = await contex.Characters.FirstOrDefaultAsync(u => u.Id == charId);
                        _userInfo.CharacterMG = false;
                    }
                    if (obj is not null)
                    {
                        _userInfo.SelectedCharacter = _mapper.Map<Character, CharacterDTO>(obj);
                        _userInfo.SelectedCharacterId = _userInfo.SelectedCharacter.Id;
                        await _protectedLocalStorage.SetAsync("SelectedCharacterId", _userInfo.SelectedCharacterId);
                        await _protectedLocalStorage.SetAsync("CharacterMG", _userInfo.CharacterMG == true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
                //    await _jsRuntime.ToastrError("Error whike getting user info: " + ex.Message);
            }
        }
    }
}
