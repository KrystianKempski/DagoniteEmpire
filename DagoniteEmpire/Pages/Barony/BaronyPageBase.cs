using DA_Business.Repository.CharacterReps.IRepository;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_DataAccess;
using DA_Models.BaronyModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DagoniteEmpire.Pages.Barony
{
    /// <summary>
    /// Shared base for barony pages: resolves current barony from user context
    /// (selected baron character), handles loading states and edit permissions.
    /// </summary>
    public abstract class BaronyPageBase : ComponentBase
    {
        [Inject] protected IUserService _userService { get; set; } = default!;
        [Inject] protected IBaronyRepository _baronyRepo { get; set; } = default!;
        [Inject] protected ISnackbar _snackBar { get; set; } = default!;
        [Inject] protected NavigationManager _navigationManager { get; set; } = default!;

        [CascadingParameter] protected BaronyViewOptions? ViewOptions { get; set; }

        protected UserInfo? UserInfo { get; set; }
        protected BaronyDTO? Barony { get; set; }
        protected int BaronyId => Barony?.Id ?? 0;

        protected bool IsLoading { get; set; } = true;
        protected string? LoadError { get; set; }
        protected bool CanEdit { get; set; }

        /// <summary>Game Master / Admin — barony structure edits (buildings, etc.).</summary>
        protected bool CanManageAsMg { get; set; }

        /// <summary>Loads barony for the currently selected baron character. Baronies are created by MG only.</summary>
        protected async Task LoadBaronyAsync()
        {
            IsLoading = true;
            LoadError = null;
            try
            {
                UserInfo = await _userService.GetUserInfo();

                var isAdminOrMg = UserInfo?.IsAdminOrMG == true;
                var isBaron = UserInfo?.Role == SD.Role_DukePlayer;

                if (!isAdminOrMg && !isBaron)
                {
                    LoadError = "Barony layer is available to Baron players and Game Masters.";
                    return;
                }

                var characterId = UserInfo?.SelectedCharacter?.Id ?? 0;
                if (characterId > 0 && characterId != -1)
                    Barony = await _baronyRepo.GetByCharacterId(characterId);
                else
                    Barony = null;

                // MG/Admin: if no barony is selected yet, pick the first available.
                if (Barony is null && isAdminOrMg)
                {
                    var summaries = await _baronyRepo.GetAllSummaries();
                    if (summaries.Count > 0)
                    {
                        var pick = summaries.FirstOrDefault(b => b.CharacterId == characterId)
                                   ?? summaries[0];
                        if (pick.CharacterId != characterId)
                        {
                            await _userService.SetSelectedCharId(pick.CharacterId);
                            UserInfo = await _userService.GetUserInfo();
                        }

                        Barony = await _baronyRepo.GetByCharacterId(pick.CharacterId);
                    }
                }

                if (Barony is null)
                {
                    if (characterId <= 0 || characterId == -1)
                    {
                        LoadError = isAdminOrMg
                            ? "Select a barony on the Domain Panel (click the barony name), or create one in Panel MG."
                            : "Select your baron character first (character menu in the top-right corner).";
                    }
                    else
                    {
                        LoadError = isAdminOrMg
                            ? "This character does not have a barony yet. Create one in Panel MG."
                            : "Your barony has not been set up yet. Ask the Game Master.";
                    }
                    return;
                }

                CanEdit = isAdminOrMg || isBaron;
                CanManageAsMg = isAdminOrMg;
            }
            catch (System.Exception ex)
            {
                LoadError = "Barony loading error: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void NotifyResourceHud() => ViewOptions?.NotifyResourceHudChanged();
    }
}
