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
    /// Wspólna baza stron baronii: rozwiązuje aktualną baronię z kontekstu użytkownika
    /// (wybrana postać barona), obsługuje stany ładowania i uprawnienia edycji.
    /// </summary>
    public abstract class BaronyPageBase : ComponentBase
    {
        [Inject] protected IUserService _userService { get; set; } = default!;
        [Inject] protected IBaronyRepository _baronyRepo { get; set; } = default!;
        [Inject] protected ISnackbar _snackBar { get; set; } = default!;
        [Inject] protected NavigationManager _navigationManager { get; set; } = default!;

        protected UserInfo? UserInfo { get; set; }
        protected BaronyDTO? Barony { get; set; }
        protected int BaronyId => Barony?.Id ?? 0;

        protected bool IsLoading { get; set; } = true;
        protected string? LoadError { get; set; }
        protected bool CanEdit { get; set; }

        /// <summary>Ładuje kontekst i (opcjonalnie) tworzy baronię, jeśli nie istnieje.</summary>
        protected async Task LoadBaronyAsync(bool createIfMissing = true)
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
                    LoadError = "Warstwa baronii jest dostępna dla graczy Baronów oraz Mistrza Gry.";
                    return;
                }

                var characterId = UserInfo?.SelectedCharacter?.Id ?? 0;
                if (characterId <= 0)
                {
                    LoadError = "Wybierz najpierw postać barona (menu postaci w prawym górnym rogu).";
                    return;
                }

                Barony = await _baronyRepo.GetByCharacterId(characterId);
                if (Barony is null && createIfMissing)
                {
                    var name = string.IsNullOrWhiteSpace(UserInfo?.SelectedCharacter?.NPCName)
                        ? "Baronia"
                        : $"Baronia — {UserInfo!.SelectedCharacter!.NPCName}";
                    Barony = await _baronyRepo.CreateForCharacter(characterId, name);
                    _snackBar.Add("Utworzono nową baronię.", Severity.Success);
                }

                if (Barony is null)
                {
                    LoadError = "Ta postać nie ma jeszcze baronii.";
                    return;
                }

                CanEdit = isAdminOrMg || isBaron;
            }
            catch (System.Exception ex)
            {
                LoadError = "Błąd ładowania baronii: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
