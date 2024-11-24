using System.Security.Claims;
using AutoMapper;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_DataAccess;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;

namespace DA_Business.Services
{
    public class UserService : IUserService
    {

        private readonly AuthenticationStateProvider _authState;
        private readonly UserInfo _userInfo;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;
        private readonly IJSRuntime _jsRuntime;


        public UserService(AuthenticationStateProvider authState, IOptions<UserInfo> userInfo, IDbContextFactory<ApplicationDbContext> db, IMapper mapper, IJSRuntime jsRuntime)
        {
            _authState = authState;
            _userInfo = userInfo.Value;
            _db = db;
            _mapper = mapper;
            _jsRuntime = jsRuntime;
        }

        
        public async Task<UserInfo?> GetUserInfo()
        {
            try
            {
                if (_userInfo is null) throw new Exception("too fast authentication");

                var user = (await _authState.GetAuthenticationStateAsync()).User;
                if (user is null)
                    throw new Exception("too fast authentication");

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
                    throw new Exception("Not authenticated");

                using var contex = await _db.CreateDbContextAsync();
                var userdb = await contex.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == _userInfo.UserName);
                if (userdb is not null)
                {
                    var obj = await contex.Characters.FirstOrDefaultAsync(u => u.Id == userdb.SelectedCharacterId);
                    if (obj is not null)
                        _userInfo.SelectedCharacter = _mapper.Map<Character,CharacterDTO>(obj);
                    else
                    {
                        obj = await contex.Characters.FirstOrDefaultAsync(u => u.UserName == _userInfo.UserName && u.IsApproved == true);
                        if (obj is not null)
                        {
                            _userInfo.SelectedCharacter = _mapper.Map<Character, CharacterDTO>(obj);
                            userdb.SelectedCharacterId = obj.Id;
                            await contex.SaveChangesAsync();
                        }
                        else
                            _userInfo.SelectedCharacter = null;
                    }
                    _userInfo.CharacterMG = _userInfo?.SelectedCharacter?.NPCName == SD.GameMaster_NPCName;

                }
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
                if (_userInfo is not null)
                {
                    using var contex = await _db.CreateDbContextAsync();
                    var user = await contex.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == _userInfo.UserName);
                    if (user != null)
                    {

                    
                        Character? obj = null;
                        if (charId>=0)
                            obj = await contex.Characters.FirstOrDefaultAsync(u => u.Id == charId);
                        else
                        {
                            obj = await contex.Characters.FirstOrDefaultAsync(u => u.NPCName == SD.GameMaster_NPCName);
                            charId = obj.Id;
                        }
                        if (obj == null) charId = 0;
                        user.SelectedCharacterId = charId;
                        var addedObj = contex.ApplicationUsers.Update(user);
                        await contex.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"error: {ex.Message}");
                //    await _jsRuntime.ToastrError("Error whike getting user info: " + ex.Message);
            }
        }
    }
}
