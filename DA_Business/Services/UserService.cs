using System.Diagnostics.Metrics;
using System.Security.Claims;
using Abp.Extensions;
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
        private readonly ProtectedSessionStorage _protectedSessionStorage;


        public UserService(AuthenticationStateProvider authState, IOptions<UserInfo> userInfo, IDbContextFactory<ApplicationDbContext> db, IMapper mapper, IJSRuntime jsRuntime, ProtectedSessionStorage protectedSessionStorage)
        {
            _authState = authState;
            _userInfo = userInfo.Value;
            _db = db;
            _mapper = mapper;
            _jsRuntime = jsRuntime;
            _protectedSessionStorage = protectedSessionStorage;
        }

        public async Task<bool> IsAuthenticated()
        {
            if (_userInfo is null) throw new Exception("too fast authentication");

            var user = (await _authState.GetAuthenticationStateAsync()).User;
            if (user is null)
                throw new Exception("too fast authentication");

            return user.Identity?.IsAuthenticated == true;
        }

        public async Task LogOut()
        {
            try
            {

            foreach(string key in ProtectedStorageKeys.All)
            {
                await _protectedSessionStorage.DeleteAsync(key);
            }
            }
            catch(Exception ex)
            {
                ;
            }
        }

        public async Task InitUserInfo()
        {
            var user = (await _authState.GetAuthenticationStateAsync()).User;
            if (user is null)
                throw new Exception("too fast authentication");

            if (user.Identity?.IsAuthenticated != true)
                return;

            await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.IsAuthenticated, true);
            await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.UserId, user.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault());
            await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.IsAdminOrMG, user.IsInRole(SD.Role_Admin) || user.IsInRole(SD.Role_GameMaster));
            await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.UserName, user.Identity?.Name);
            string role = string.Empty;
            if (user.IsInRole(SD.Role_HeroPlayer))
            {
                role = SD.Role_HeroPlayer;
            }
            else if (user.IsInRole(SD.Role_DukePlayer))
            {
                role = SD.Role_DukePlayer;
            }
            else if (user.IsInRole(SD.Role_Admin))
            {
                role = SD.Role_Admin;
            }
            else if (user.IsInRole(SD.Role_GameMaster))
            {
                role = SD.Role_GameMaster;
            }
            await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.Role, role);
            await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.IsInited, true);
            //_userInfo.IsInited = true;
        }

        public async Task<UserInfo?> GetUserInfo()
        {
            try
            {
                UserInfo userInfo = new UserInfo();

                if (userInfo == null) return null;

                var resultIsInited = await _protectedSessionStorage.GetAsync<bool>(ProtectedStorageKeys.IsInited);
                bool isInited = resultIsInited.Success ? resultIsInited.Value : false;
                if (isInited == false)
                    await InitUserInfo();

                var resultIsAuthenticated = await _protectedSessionStorage.GetAsync<bool?>(ProtectedStorageKeys.IsAuthenticated);
                userInfo.IsAuthenticated = resultIsAuthenticated.Success ? resultIsAuthenticated.Value : null;
                if (userInfo.IsAuthenticated == false)
                    return null;



                var resultUserId = await _protectedSessionStorage.GetAsync<string?>(ProtectedStorageKeys.UserId);
                userInfo.UserId = resultUserId.Success ? resultUserId.Value : null;

                var resultIsAdminOrMG = await _protectedSessionStorage.GetAsync<bool?>(ProtectedStorageKeys.IsAdminOrMG);
                userInfo.IsAdminOrMG = resultIsAdminOrMG.Success ? resultIsAdminOrMG.Value : null;

                var resultUserName = await _protectedSessionStorage.GetAsync<string?>(ProtectedStorageKeys.UserName);
                userInfo.UserName = resultUserName.Success ? resultUserName.Value : null;

                var resultRole = await _protectedSessionStorage.GetAsync<string?>(ProtectedStorageKeys.Role);
                userInfo.Role = resultRole.Success ? resultRole.Value : null;

                var resultSelectedCharacterId = await _protectedSessionStorage.GetAsync<int>(ProtectedStorageKeys.SelectedCharacterId);
                userInfo.SelectedCharacterId = resultSelectedCharacterId.Success ? resultSelectedCharacterId.Value : 0;
                if(userInfo.SelectedCharacterId == 0)
                {
                    userInfo.SelectedCharacter = null;
                }
                
                else
                {
                    if (userInfo.SelectedCharacter is null || userInfo.SelectedCharacter.Id != userInfo.SelectedCharacterId)
                    {
                        using var contex = await _db.CreateDbContextAsync();
                        Character? obj = null;
                        if(userInfo.SelectedCharacterId == -1)
                        { 
                            obj = await contex.Characters.FirstOrDefaultAsync(u => u.NPCName == SD.GameMaster_NPCName);
                        }
                        else
                        {
                            obj = await contex.Characters.FirstOrDefaultAsync(u => u.Id == userInfo.SelectedCharacterId);
                        }
                        if (obj == null)
                        {
                            throw new Exception($"Error! No character found");
                        }
                        userInfo.SelectedCharacter = _mapper.Map<Character, CharacterDTO>(obj);
                    }
                }
                var resultCharacterMG = await _protectedSessionStorage.GetAsync<bool?>(ProtectedStorageKeys.CharacterMG);
                userInfo.CharacterMG = resultCharacterMG.Success ? resultCharacterMG.Value : null;

                return userInfo;
                
            }
            catch (Exception ex)
            {
                //throw new Exception($"error: {ex.Message}");
                ;
                return null;
            }
        }


        public async Task SetSelectedCharId(int charId)
        {
            try
            {
                var resultSelectedCharacterId = await _protectedSessionStorage.GetAsync<int>(ProtectedStorageKeys.SelectedCharacterId);
                int currentCharacterId = resultSelectedCharacterId.Success ? resultSelectedCharacterId.Value : 0;
                if (charId != currentCharacterId)
                {
                    bool characterMG = false;
                    if (charId == -1)
                    {
                        characterMG = true;
                    }

                    await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.SelectedCharacterId, charId);
                    await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.CharacterMG, characterMG);
                    await _protectedSessionStorage.SetAsync(ProtectedStorageKeys.IsInited, false);
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
