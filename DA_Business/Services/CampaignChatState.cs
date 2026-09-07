using DA_Business.Repository.ChatRepos.IRepository;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Business.Services.Interfaces;
using DA_Common;
using DA_DataAccess;
using DA_DataAccess.Chat;
using DA_Models.ChatModels;

namespace DA_Business.Services
{
    /// <summary>
    /// Per-circuit state for the campaign chat drawer and AppBar launcher.
    /// </summary>
    public class CampaignChatState : IAsyncDisposable
    {
        private readonly ICampaignChatRepository _chat;
        private readonly ICampaignRepository _campaigns;
        private readonly IUserService _userService;
        private readonly ICampaignChatBroadcaster _broadcaster;
        private readonly CallbackService _callback;
        private IDisposable? _broadcastSub;
        private bool _initialized;

        public CampaignChatState(
            ICampaignChatRepository chat,
            ICampaignRepository campaigns,
            IUserService userService,
            ICampaignChatBroadcaster broadcaster,
            CallbackService callback)
        {
            _chat = chat;
            _campaigns = campaigns;
            _userService = userService;
            _broadcaster = broadcaster;
            _callback = callback;
            _callback.OnChange += OnCharacterChanged;
        }

        public bool IsOpen { get; private set; }
        public bool IsVisible { get; private set; }
        public int UnreadTotal { get; private set; }
        public int? SelectedCampaignId { get; private set; }
        public int? SelectedPeerCharacterId { get; private set; }
        public bool IsInThread => _inThread;
        public IReadOnlyList<CampaignDTO> Campaigns { get; private set; } = Array.Empty<CampaignDTO>();
        public IReadOnlyList<CampaignChatContactDTO> Contacts { get; private set; } = Array.Empty<CampaignChatContactDTO>();
        public IReadOnlyList<CampaignChatMessageDTO> Messages { get; private set; } = Array.Empty<CampaignChatMessageDTO>();
        public bool HasMoreMessages { get; private set; }
        public bool IsLoading { get; private set; }
        public string? Error { get; private set; }

        private bool _inThread;
        private int _myCharacterId;
        private string _userName = string.Empty;
        private bool _isAdminOrMg;
        private int? _pendingDeepLinkCampaignId;
        private int? _pendingDeepLinkPeerId;

        public event Action? OnChange;

        public async Task EnsureInitializedAsync()
        {
            if (_initialized)
                return;
            _initialized = true;
            await ReloadIdentityAsync();
        }

        public async Task OpenAsync()
        {
            await EnsureInitializedAsync();
            IsOpen = true;
            if (SelectedCampaignId is null && Campaigns.Count > 0)
                SelectedCampaignId = Campaigns[0].Id;
            if (SelectedCampaignId is not null)
                await LoadContactsAsync();
            Notify();
        }

        public void Close()
        {
            IsOpen = false;
            Notify();
        }

        public void Toggle()
        {
            if (IsOpen)
                Close();
            else
                _ = OpenAsync();
        }

        public async Task SelectCampaignAsync(int campaignId)
        {
            if (SelectedCampaignId == campaignId && Contacts.Count > 0)
                return;

            SelectedCampaignId = campaignId;
            _inThread = false;
            SelectedPeerCharacterId = null;
            Messages = Array.Empty<CampaignChatMessageDTO>();
            Resubscribe();
            await LoadContactsAsync();
            Notify();
        }

        public async Task OpenThreadAsync(int? peerCharacterId)
        {
            if (SelectedCampaignId is null)
                return;

            SelectedPeerCharacterId = peerCharacterId;
            _inThread = true;
            IsLoading = true;
            Error = null;
            Notify();

            try
            {
                Messages = await _chat.GetThreadAsync(
                    SelectedCampaignId.Value,
                    _myCharacterId,
                    peerCharacterId,
                    _userName,
                    _isAdminOrMg);
                HasMoreMessages = Messages.Count >= 50;
                await _chat.MarkReadAsync(
                    SelectedCampaignId.Value,
                    _myCharacterId,
                    peerCharacterId,
                    _userName,
                    _isAdminOrMg);
                await RefreshUnreadAsync();
                await LoadContactsAsync(silent: true);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            finally
            {
                IsLoading = false;
                Notify();
            }
        }

        public void BackToContacts()
        {
            _inThread = false;
            SelectedPeerCharacterId = null;
            Messages = Array.Empty<CampaignChatMessageDTO>();
            Notify();
            _ = LoadContactsAsync();
        }

        public async Task SendAsync(string content)
        {
            if (SelectedCampaignId is null || string.IsNullOrWhiteSpace(content))
                return;

            var dto = await _chat.SendAsync(
                SelectedCampaignId.Value,
                _myCharacterId,
                SelectedPeerCharacterId,
                content,
                _userName,
                _isAdminOrMg);

            if (Messages.All(m => m.Id != dto.Id))
                Messages = Messages.Append(dto).ToList();

            await LoadContactsAsync(silent: true);
            Notify();
        }

        public async Task LoadOlderAsync()
        {
            if (SelectedCampaignId is null || Messages.Count == 0 || !HasMoreMessages)
                return;

            var older = await _chat.GetThreadAsync(
                SelectedCampaignId.Value,
                _myCharacterId,
                SelectedPeerCharacterId,
                _userName,
                _isAdminOrMg,
                take: 50,
                beforeId: Messages[0].Id);

            HasMoreMessages = older.Count >= 50;
            if (older.Count > 0)
                Messages = older.Concat(Messages).ToList();
            Notify();
        }

        /// <summary>
        /// Deep-link from push: <c>?chat={campaignId}:{peerCharacterId}</c>.
        /// Empty peer opens the party channel.
        /// </summary>
        public async Task ApplyDeepLinkAsync(int campaignId, int? peerCharacterId)
        {
            await EnsureInitializedAsync();
            if (!IsVisible)
            {
                _pendingDeepLinkCampaignId = campaignId;
                _pendingDeepLinkPeerId = peerCharacterId;
                return;
            }

            IsOpen = true;
            await SelectCampaignAsync(campaignId);
            await OpenThreadAsync(peerCharacterId);
        }

        public async Task RefreshUnreadAsync()
        {
            if (_myCharacterId <= 0 || string.IsNullOrWhiteSpace(_userName))
            {
                UnreadTotal = 0;
                return;
            }

            try
            {
                UnreadTotal = await _chat.GetUnreadSummaryAsync(_myCharacterId, _userName, _isAdminOrMg);
            }
            catch
            {
                UnreadTotal = 0;
            }
            Notify();
        }

        private async Task ReloadIdentityAsync()
        {
            var user = await _userService.GetUserInfo();
            if (user?.IsAuthenticated != true || user.SelectedCharacter is null)
            {
                IsVisible = false;
                UnreadTotal = 0;
                Campaigns = Array.Empty<CampaignDTO>();
                DisposeSubscription();
                Notify();
                return;
            }

            _userName = user.UserName ?? string.Empty;
            _isAdminOrMg = user.IsAdminOrMG == true;
            _myCharacterId = user.SelectedCharacter.Id;

            if (_myCharacterId <= 0)
            {
                IsVisible = false;
                Notify();
                return;
            }

            IEnumerable<CampaignDTO> campaigns;
            if (_isAdminOrMg && user.CharacterMG == true)
            {
                campaigns = await _campaigns.GetAll();
            }
            else
            {
                campaigns = await _campaigns.GetAll(_myCharacterId);
            }

            Campaigns = campaigns
                .Where(c => !c.IsFinished)
                .OrderBy(c => c.Name)
                .ToList();

            IsVisible = Campaigns.Count > 0;
            if (!IsVisible)
            {
                IsOpen = false;
                DisposeSubscription();
                UnreadTotal = 0;
                Notify();
                return;
            }

            if (SelectedCampaignId is null || Campaigns.All(c => c.Id != SelectedCampaignId))
                SelectedCampaignId = Campaigns[0].Id;

            Resubscribe();
            await RefreshUnreadAsync();

            if (_pendingDeepLinkCampaignId is int pendingCampaign)
            {
                var peer = _pendingDeepLinkPeerId;
                _pendingDeepLinkCampaignId = null;
                _pendingDeepLinkPeerId = null;
                IsOpen = true;
                await SelectCampaignAsync(pendingCampaign);
                await OpenThreadAsync(peer);
            }

            Notify();
        }

        private async Task LoadContactsAsync(bool silent = false)
        {
            if (SelectedCampaignId is null || _myCharacterId <= 0)
                return;

            if (!silent)
            {
                IsLoading = true;
                Notify();
            }

            try
            {
                Contacts = await _chat.GetContactsAsync(
                    SelectedCampaignId.Value,
                    _myCharacterId,
                    _userName,
                    _isAdminOrMg);
                Error = null;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                Contacts = Array.Empty<CampaignChatContactDTO>();
            }
            finally
            {
                if (!silent)
                    IsLoading = false;
                Notify();
            }
        }

        private void Resubscribe()
        {
            DisposeSubscription();
            if (SelectedCampaignId is null || _myCharacterId <= 0)
                return;

            _broadcastSub = _broadcaster.Subscribe(
                SelectedCampaignId.Value,
                _myCharacterId,
                OnBroadcastAsync);
        }

        private async Task OnBroadcastAsync(CampaignChatMessageDTO message)
        {
            if (SelectedCampaignId != message.CampaignId)
                return;

            var viewingThisThread = _inThread && (
                (SelectedPeerCharacterId is null && message.RecipientCharacterId is null)
                || (SelectedPeerCharacterId is int peer
                    && message.RecipientCharacterId is not null
                    && ((message.SenderCharacterId == _myCharacterId && message.RecipientCharacterId == peer)
                        || (message.SenderCharacterId == peer && message.RecipientCharacterId == _myCharacterId))));

            if (viewingThisThread)
            {
                if (Messages.All(m => m.Id != message.Id))
                {
                    var copy = message;
                    copy.IsMine = message.SenderCharacterId == _myCharacterId;
                    Messages = Messages.Append(copy).ToList();
                }

                if (!message.IsMine && message.SenderCharacterId != _myCharacterId)
                {
                    try
                    {
                        await _chat.MarkReadAsync(
                            message.CampaignId,
                            _myCharacterId,
                            SelectedPeerCharacterId,
                            _userName,
                            _isAdminOrMg);
                    }
                    catch { /* ignore */ }
                }
            }
            else if (message.SenderCharacterId != _myCharacterId)
            {
                UnreadTotal++;
            }

            await LoadContactsAsync(silent: true);
            Notify();
        }

        private void OnCharacterChanged() => _ = ReloadIdentityAsync();

        private void DisposeSubscription()
        {
            _broadcastSub?.Dispose();
            _broadcastSub = null;
        }

        private void Notify() => OnChange?.Invoke();

        public ValueTask DisposeAsync()
        {
            _callback.OnChange -= OnCharacterChanged;
            DisposeSubscription();
            return ValueTask.CompletedTask;
        }
    }
}
