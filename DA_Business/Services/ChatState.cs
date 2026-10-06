using System.Security.Claims;
using DA_Business.Repository.ChatRepos.IRepository;
using DA_Business.Services.Interfaces;
using DA_Models.ChatModels;
using Microsoft.AspNetCore.Components.Authorization;

namespace DA_Business.Services
{
    /// <summary>
    /// Per-circuit state for the chat drawer and the AppBar launcher.
    ///
    /// Identity comes from the circuit's claims, never from the selected character or from
    /// <c>ProtectedSessionStorage</c>: chat belongs to the account, so switching characters or campaigns
    /// must not change what the drawer shows or mute the unread badge. That also means there is nothing
    /// to re-read after the first render, which is why this class has no identity-reload races.
    /// </summary>
    public class ChatState : IAsyncDisposable
    {
        private readonly IChatRepository _chat;
        private readonly IChatBroadcaster _broadcaster;
        private readonly AuthenticationStateProvider _authState;

        private IDisposable? _subscription;
        private Task? _initialization;
        private bool _inThread;
        private string _userId = string.Empty;

        public ChatState(
            IChatRepository chat,
            IChatBroadcaster broadcaster,
            AuthenticationStateProvider authState)
        {
            _chat = chat;
            _broadcaster = broadcaster;
            _authState = authState;
        }

        public bool IsOpen { get; private set; }
        public bool IsVisible { get; private set; }
        public int UnreadTotal { get; private set; }
        public long? SelectedConversationId { get; private set; }
        public string? ThreadTitle { get; private set; }
        public bool IsInThread => _inThread;
        public IReadOnlyList<ChatContactDTO> Contacts { get; private set; } = Array.Empty<ChatContactDTO>();
        public IReadOnlyList<ChatMessageDTO> Messages { get; private set; } = Array.Empty<ChatMessageDTO>();
        public bool HasMoreMessages { get; private set; }
        public bool IsLoading { get; private set; }
        public string? Error { get; private set; }

        public event Action? OnChange;

        /// <summary>
        /// The launcher, the drawer and the push deep link all call this. Caching the task rather than a
        /// bool means a late caller waits for the first load to finish instead of racing past it and
        /// reading a half-built state.
        /// </summary>
        public Task EnsureInitializedAsync() => _initialization ??= InitializeAsync();

        private async Task InitializeAsync()
        {
            _userId = await ResolveUserIdAsync();
            if (string.IsNullOrWhiteSpace(_userId))
            {
                IsVisible = false;
                Notify();
                return;
            }

            _subscription = _broadcaster.Subscribe(_userId, OnBroadcastAsync);

            await LoadContactsAsync();
            IsVisible = Contacts.Count > 0;
            Notify();
        }

        public async Task OpenAsync()
        {
            await EnsureInitializedAsync();
            if (!IsVisible)
                return;

            IsOpen = true;
            await LoadContactsAsync(silent: true);
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

        public void BackToContacts()
        {
            _inThread = false;
            SelectedConversationId = null;
            ThreadTitle = null;
            Messages = Array.Empty<ChatMessageDTO>();
            Notify();
            _ = LoadContactsAsync(silent: true);
        }

        /// <summary>
        /// Opens a row of the chat list, creating the thread if this is the first message to that peer or
        /// campaign — threads are not created just by looking at the list.
        /// </summary>
        public async Task OpenContactAsync(ChatContactDTO contact)
        {
            if (string.IsNullOrWhiteSpace(_userId))
                return;

            try
            {
                var conversationId = contact.ConversationId
                    ?? (contact.IsPartyChannel
                        ? await _chat.GetOrCreatePartyAsync(_userId, contact.CampaignId ?? 0)
                        : await _chat.GetOrCreateDirectAsync(_userId, contact.PeerUserId, contact.PeerIsGameMaster));

                ThreadTitle = contact.DisplayName;
                await OpenConversationAsync(conversationId);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                Notify();
            }
        }

        public async Task OpenConversationAsync(long conversationId)
        {
            if (string.IsNullOrWhiteSpace(_userId))
                return;

            SelectedConversationId = conversationId;
            _inThread = true;
            IsLoading = true;
            Error = null;
            Notify();

            try
            {
                Messages = await _chat.GetThreadAsync(conversationId, _userId);
                HasMoreMessages = Messages.Count >= 50;

                ThreadTitle ??= await ResolveTitleAsync(conversationId);

                await _chat.MarkReadAsync(conversationId, _userId);
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

        public async Task SendAsync(string content)
        {
            if (SelectedConversationId is not long conversationId
                || string.IsNullOrWhiteSpace(content)
                || string.IsNullOrWhiteSpace(_userId))
                return;

            try
            {
                var dto = await _chat.SendAsync(conversationId, _userId, content);
                if (Messages.All(m => m.Id != dto.Id))
                    Messages = Messages.Append(dto).ToList();

                await LoadContactsAsync(silent: true);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            Notify();
        }

        public async Task LoadOlderAsync()
        {
            if (SelectedConversationId is not long conversationId
                || Messages.Count == 0
                || !HasMoreMessages)
                return;

            var older = await _chat.GetThreadAsync(conversationId, _userId, take: 50, beforeId: Messages[0].Id);

            HasMoreMessages = older.Count >= 50;
            if (older.Count > 0)
                Messages = older.Concat(Messages).ToList();

            Notify();
        }

        /// <summary>Deep-link from push: <c>?chat={conversationId}</c>.</summary>
        public async Task ApplyDeepLinkAsync(long conversationId)
        {
            if (conversationId <= 0)
                return;

            await EnsureInitializedAsync();
            if (!IsVisible)
                return;

            IsOpen = true;
            ThreadTitle = null;
            await OpenConversationAsync(conversationId);
        }

        private async Task LoadContactsAsync(bool silent = false)
        {
            if (string.IsNullOrWhiteSpace(_userId))
                return;

            if (!silent)
            {
                IsLoading = true;
                Notify();
            }

            try
            {
                Contacts = await _chat.GetContactsAsync(_userId);
                UnreadTotal = Contacts.Sum(c => c.UnreadCount);
                Error = null;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            finally
            {
                if (!silent)
                    IsLoading = false;
                Notify();
            }
        }

        private async Task OnBroadcastAsync(ChatMessageDTO message)
        {
            if (_inThread
                && SelectedConversationId == message.ConversationId
                && Messages.All(m => m.Id != message.Id))
            {
                Messages = Messages.Append(ForMe(message)).ToList();

                if (!string.Equals(message.SenderUserId, _userId, StringComparison.Ordinal))
                {
                    try
                    {
                        await _chat.MarkReadAsync(message.ConversationId, _userId);
                    }
                    catch
                    {
                        // Watermark is a nicety; a failed write must not break the live update.
                    }
                }
            }

            await LoadContactsAsync(silent: true);
            Notify();
        }

        private async Task<string?> ResolveTitleAsync(long conversationId)
        {
            var known = Contacts.FirstOrDefault(c => c.ConversationId == conversationId);
            if (known is not null)
                return known.DisplayName;

            var info = await _chat.GetConversationInfoAsync(conversationId, _userId);
            return info?.DisplayName;
        }

        private async Task<string> ResolveUserIdAsync()
        {
            try
            {
                var state = await _authState.GetAuthenticationStateAsync();
                if (state.User.Identity?.IsAuthenticated != true)
                    return string.Empty;

                return state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The bus hands the same instance to every circuit, so <see cref="ChatMessageDTO.IsMine"/> has to
        /// be decided on a copy — writing it in place would let one reader flip the bubble alignment for
        /// everyone else.
        /// </summary>
        private ChatMessageDTO ForMe(ChatMessageDTO message) => new()
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            SenderName = message.SenderName,
            SenderImageUrl = message.SenderImageUrl,
            SentAsGameMaster = message.SentAsGameMaster,
            Content = message.Content,
            CreatedUtc = message.CreatedUtc,
            IsMine = string.Equals(message.SenderUserId, _userId, StringComparison.Ordinal),
        };

        private void Notify() => OnChange?.Invoke();

        public ValueTask DisposeAsync()
        {
            _subscription?.Dispose();
            _subscription = null;
            return ValueTask.CompletedTask;
        }
    }
}
