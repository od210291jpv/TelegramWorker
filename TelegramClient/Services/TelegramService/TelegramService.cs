using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace TelegramClient.Services.TelegramService
{
    public class TelegramService : ITelegramService
    {
        private Client tgClient;

        private const int api_id = 12146829;
        private const string api_hash = "53f6b554fe56bbb3bfcc6866a48714b3";

        public TelegramService()
        {
            this.tgClient = new WTelegram.Client(what =>
            {
                if (what == "api_id") return api_id.ToString();
                if (what == "api_hash") return api_hash;
                if (what == "phone_number") { Console.Write("Enter phone number (e.g., +1234567890): "); return Console.ReadLine(); }
                if (what == "verification_code") { Console.Write("Enter verification code: "); return Console.ReadLine(); }
                if (what == "password") { Console.Write("Enter 2FA password (if any): "); return Console.ReadLine(); }
                return null;
            });
        }

        public async Task<User> LoginAsync()
        {
            return await this.tgClient.LoginUserIfNeeded();
        }

        public async Task<Contacts_Contacts> GetAllContactsAsync(CancellationToken cancellationToken = default)
        {
            // 0 hash means "fetch full list" without using local cache hash
            return (Contacts_Contacts)await this.tgClient.Contacts_GetContacts(0);
        }

        public async Task<Messages_DialogsBase> GetAllChatsAsync(CancellationToken cancellationToken = default)
        {
            // Retrieves all dialogs (which includes all chats, groups, channels, and PMs)
            return await this.tgClient.Messages_GetDialogs(
                offset_date: default,
                offset_id: 0,
                offset_peer: null,
                limit: 100,
                hash: 0);
        }

        public async Task<Messages_InvitedUsers> CreateChatAsync(string title, IEnumerable<InputUser> users, CancellationToken cancellationToken = default)
        {
            var usersArray = new List<InputUser>(users).ToArray();
            return await this.tgClient.Messages_CreateChat(usersArray, title);
        }

        public async Task<ChatBase> GetChatAsync(InputPeer peer, CancellationToken cancellationToken = default)
        {
            if (peer is InputPeerChannel channel)
            {
                var chats = await this.tgClient.Channels_GetChannels(new[] { (InputChannel)channel });
                return chats.chats.Values.FirstOrDefault();
            }
            else if (peer is InputPeerChat chat)
            {
                var chats = await this.tgClient.Messages_GetChats(new[] { chat.chat_id });
                return chats.chats.Values.FirstOrDefault();
            }
            return null;
        }

        public async Task<UpdatesBase> UpdateChatTitleAsync(InputPeer peer, string newTitle, CancellationToken cancellationToken = default)
        {
            if (peer is InputPeerChannel channel)
            {
                return await this.tgClient.Channels_EditTitle(channel, newTitle);
            }
            else if (peer is InputPeerChat chat)
            {
                return await this.tgClient.Messages_EditChatTitle(chat.chat_id, newTitle);
            }
            throw new ArgumentException("Unsupported peer type for chat title update");
        }

        public async Task<UpdatesBase> DeleteChatAsync(InputPeer peer, CancellationToken cancellationToken = default)
        {
            if (peer is InputPeerChannel channel)
            {
                return await this.tgClient.Channels_DeleteChannel(channel);
            }
            else if (peer is InputPeerChat chat)
            {
                var me = await this.tgClient.Contacts_ResolveUsername("me"); // get self
                return await this.tgClient.Messages_DeleteChatUser(chat.chat_id, new InputUserSelf());
            }
            throw new ArgumentException("Unsupported peer type for chat deletion");
        }

        public async Task<Message> SendMessageAsync(InputPeer peer, string text, CancellationToken cancellationToken = default)
        {
            return await this.tgClient.SendMessageAsync(peer, text);
        }

        public async Task<Message> GetMessageAsync(InputPeer peer, int messageId, CancellationToken cancellationToken = default)
        {
            var msgId = new InputMessageID { id = messageId };
            Messages_MessagesBase result;

            if (peer is InputPeerChannel channel)
            {
                result = await this.tgClient.Channels_GetMessages(channel, new InputMessage[] { msgId });
            }
            else
            {
                result = await this.tgClient.Messages_GetMessages(new InputMessage[] { msgId });
            }

            return result.Messages.OfType<Message>().FirstOrDefault();
        }

        public async Task<Messages_MessagesBase> GetChatHistoryAsync(InputPeer peer, int limit = 100, int offsetId = 0, CancellationToken cancellationToken = default)
        {
            return await this.tgClient.Messages_GetHistory(
                peer: peer,
                offset_id: offsetId,
                offset_date: default,
                add_offset: 0,
                limit: limit,
                max_id: 0,
                min_id: 0,
                hash: 0
            );
        }

        public async Task<UpdatesBase> UpdateMessageAsync(InputPeer peer, int messageId, string newText, CancellationToken cancellationToken = default)
        {
            return await this.tgClient.Messages_EditMessage(peer, messageId, newText);
        }

        public async Task<object> DeleteMessagesAsync(InputPeer peer, IEnumerable<int> messageIds, CancellationToken cancellationToken = default)
        {
            var ids = new List<int>(messageIds).ToArray();
            if (peer is InputPeerChannel channel)
            {
                return await this.tgClient.Channels_DeleteMessages(channel, ids);
            }
            else
            {
                return await this.tgClient.Messages_DeleteMessages(ids, revoke: true);
            }
        }

        public async Task<Message> SendMediaAsync(InputPeer peer, Stream fileStream, string filename, string caption = null, CancellationToken cancellationToken = default)
        {
            var inputFile = await this.tgClient.UploadFileAsync(fileStream, filename);
            var media = new InputMediaUploadedDocument
            {
                file = inputFile,
                mime_type = "application/octet-stream",
                attributes = new[] { new DocumentAttributeFilename { file_name = filename } }
            };
            return await this.tgClient.SendMessageAsync(peer, caption ?? "", media);
        }

        public async Task<Stream> DownloadMediaAsync(Message message, CancellationToken cancellationToken = default)
        {
            var stream = new MemoryStream();
            if (message.media is MessageMediaDocument docMedia && docMedia.document is Document doc)
            {
                await this.tgClient.DownloadFileAsync(doc, stream);
            }
            else if (message.media is MessageMediaPhoto photoMedia && photoMedia.photo is Photo photo)
            {
                await this.tgClient.DownloadFileAsync(photo, stream);
            }
            stream.Position = 0;
            return stream;
        }

        public async Task<Messages_MessagesBase> SearchMessagesAsync(InputPeer peer, string query, int limit = 100, CancellationToken cancellationToken = default)
        {
            return await this.tgClient.Messages_Search(peer, query, null, limit: limit);
        }

        public async Task<Contacts_Found> SearchGlobalAsync(string query, int limit = 100, CancellationToken cancellationToken = default)
        {
            return await this.tgClient.Contacts_Search(query, limit);
        }

        public async Task<IEnumerable<User>> FindUsersAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
        {
            // This hits Telegram's servers to search your contacts and the global directory
            // by first name, last name, or username.
            var found = await this.tgClient.Contacts_Search(query, limit);
            
            // Return only the users found (ignoring chats/channels that might match)
            return found.users.Values;
        }

        public async Task<object> AddUserToChatAsync(ChatBase chat, InputUser user, CancellationToken cancellationToken = default)
        {
            if (chat is Channel channel)
            {
                return await this.tgClient.Channels_InviteToChannel(channel, new[] { user });
            }
            else if (chat is Chat basicChat)
            {
                return await this.tgClient.Messages_AddChatUser(basicChat.id, user, fwd_limit: 50);
            }
            throw new ArgumentException("Unsupported chat type");
        }

        public async Task<UpdatesBase> RemoveUserFromChatAsync(ChatBase chat, InputUser user, CancellationToken cancellationToken = default)
        {
            if (chat is Channel channel)
            {
                // To remove someone from a channel/supergroup, you kick them
                return await this.tgClient.Channels_EditBanned(channel, user, new ChatBannedRights
                {
                    flags = ChatBannedRights.Flags.view_messages
                });
            }
            else if (chat is Chat basicChat)
            {
                return await this.tgClient.Messages_DeleteChatUser(basicChat.id, user);
            }
            throw new ArgumentException("Unsupported chat type");
        }

        public async Task<UpdatesBase> PromoteUserAsync(ChatBase chat, InputUser user, ChatAdminRights adminRights, CancellationToken cancellationToken = default)
        {
            if (chat is Channel channel)
            {
                return await this.tgClient.Channels_EditAdmin(channel, user, adminRights, rank: "");
            }
            throw new ArgumentException("Promoting users is only supported in supergroups/channels");
        }

        public async Task<UpdatesBase> DemoteUserAsync(ChatBase chat, InputUser user, CancellationToken cancellationToken = default)
        {
            if (chat is Channel channel)
            {
                return await this.tgClient.Channels_EditAdmin(channel, user, new ChatAdminRights(), rank: "");
            }
            throw new ArgumentException("Demoting users is only supported in supergroups/channels");
        }

        public async Task<UpdatesBase> BanUserAsync(ChatBase chat, InputUser user, ChatBannedRights bannedRights, CancellationToken cancellationToken = default)
        {
            if (chat is Channel channel)
            {
                return await this.tgClient.Channels_EditBanned(channel, user, bannedRights);
            }
            throw new ArgumentException("Banning users is only supported in supergroups/channels");
        }

        public async Task<UpdatesBase> UnbanUserAsync(ChatBase chat, InputUser user, CancellationToken cancellationToken = default)
        {
            if (chat is Channel channel)
            {
                return await this.tgClient.Channels_EditBanned(channel, user, new ChatBannedRights()); // empty rights = unban
            }
            throw new ArgumentException("Unbanning users is only supported in supergroups/channels");
        }

        public async Task<Dictionary<long, User>> GetChatMembersAsync(ChatBase chat, CancellationToken cancellationToken = default)
        {
            if (chat is Channel channel)
            {
                var participants = await this.tgClient.Channels_GetParticipants(channel, new ChannelParticipantsRecent(), offset: 0, limit: 1000, hash: 0);
                return participants.users;
            }
            else if (chat is Chat basicChat)
            {
                var fullChat = await this.tgClient.Messages_GetFullChat(basicChat.id);
                return fullChat.users;
            }
            return new Dictionary<long, User>();
        }
    }
}
