using TL;

namespace TelegramClient.Services.TelegramService
{
    public interface ITelegramService
    {
        // --- Setup ---
        Task<User> LoginAsync();

        // --- Contacts ---
        Task<Contacts_Contacts> GetAllContactsAsync(CancellationToken cancellationToken = default);

        // --- Chats CRUD ---
        Task<Messages_DialogsBase> GetAllChatsAsync(CancellationToken cancellationToken = default);
        Task<Messages_InvitedUsers> CreateChatAsync(string title, IEnumerable<InputUser> users, CancellationToken cancellationToken = default);
        Task<ChatBase> GetChatAsync(InputPeer peer, CancellationToken cancellationToken = default);
        Task<UpdatesBase> UpdateChatTitleAsync(InputPeer peer, string newTitle, CancellationToken cancellationToken = default);
        Task<UpdatesBase> DeleteChatAsync(InputPeer peer, CancellationToken cancellationToken = default);

        // --- Messages CRUD ---
        Task<Messages_MessagesBase> GetChatHistoryAsync(InputPeer peer, int limit = 100, int offsetId = 0, CancellationToken cancellationToken = default);
        Task<Message> SendMessageAsync(InputPeer peer, string text, CancellationToken cancellationToken = default);
        Task<Message> GetMessageAsync(InputPeer peer, int messageId, CancellationToken cancellationToken = default);
        Task<UpdatesBase> UpdateMessageAsync(InputPeer peer, int messageId, string newText, CancellationToken cancellationToken = default);
        Task<object> DeleteMessagesAsync(InputPeer peer, IEnumerable<int> messageIds, CancellationToken cancellationToken = default);

        // --- Media CRUD (Images & Videos) ---
        Task<Message> SendMediaAsync(InputPeer peer, Stream fileStream, string filename, string caption = null, CancellationToken cancellationToken = default);
        Task<Stream> DownloadMediaAsync(Message message, CancellationToken cancellationToken = default);

        // --- Search ---
        Task<Messages_MessagesBase> SearchMessagesAsync(InputPeer peer, string query, int limit = 100, CancellationToken cancellationToken = default);
        Task<Contacts_Found> SearchGlobalAsync(string query, int limit = 100, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> FindUsersAsync(string query, int limit = 10, CancellationToken cancellationToken = default);

        // --- User Management in Owned Chats ---
        Task<object> AddUserToChatAsync(ChatBase chat, InputUser user, CancellationToken cancellationToken = default);
        Task<UpdatesBase> RemoveUserFromChatAsync(ChatBase chat, InputUser user, CancellationToken cancellationToken = default);
        Task<UpdatesBase> PromoteUserAsync(ChatBase chat, InputUser user, ChatAdminRights adminRights, CancellationToken cancellationToken = default);
        Task<UpdatesBase> DemoteUserAsync(ChatBase chat, InputUser user, CancellationToken cancellationToken = default);
        Task<UpdatesBase> BanUserAsync(ChatBase chat, InputUser user, ChatBannedRights bannedRights, CancellationToken cancellationToken = default);
        Task<UpdatesBase> UnbanUserAsync(ChatBase chat, InputUser user, CancellationToken cancellationToken = default);
        Task<Dictionary<long, User>> GetChatMembersAsync(ChatBase chat, CancellationToken cancellationToken = default);
    }
}
