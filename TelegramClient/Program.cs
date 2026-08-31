using TelegramClient.Services.TelegramService;
using TL;

namespace TelegramUserClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Telegram Client...");

            var telegramService = new TelegramService();
            
            try
            {
                // 1. Setup & Login
                Console.WriteLine("\n=== 1. Setup & Login ===");
                var me = await telegramService.LoginAsync();
                Console.WriteLine($"Successfully logged in as {me.first_name} (ID: {me.id})");

                // Target peer for testing (Using your own "Saved Messages")
                InputPeer targetPeer = me;

                // 2. Contacts & Search
                Console.WriteLine("\n=== 2. Contacts & Search ===");

                Messages_DialogsBase dialogsResult = await telegramService.GetAllChatsAsync();

                foreach (var dialog in dialogsResult.Dialogs)
                {
                    var peerInfo = dialogsResult.UserOrChat(dialog.Peer);
                    if (peerInfo is ChatBase chat) 
                    {
                        Console.WriteLine($"Dialog: {chat.MainUsername} {chat.Title} {chat.ID}");
                    }
                }

                var contacts = await telegramService.GetAllContactsAsync();
                Console.WriteLine($"Found {contacts.users.Count} contacts.");

                var searchResults = await telegramService.FindUsersAsync("Telegram");
                var someUser = searchResults.FirstOrDefault();
                Console.WriteLine($"Global Search for 'Telegram' found: {someUser?.first_name}");

                // 3. Messages CRUD
                Console.WriteLine("\n=== 3. Messages CRUD ===");

                // Create (Send) Message
                var sentMsg = await telegramService.SendMessageAsync(targetPeer, "Hello from TelegramService!");
                Console.WriteLine($"Message sent with ID: {sentMsg.id}");

                // Read (Get) Message
                var fetchedMsg = await telegramService.GetMessageAsync(targetPeer, sentMsg.id);
                Console.WriteLine($"Fetched message text: {fetchedMsg?.message}");

                // Update (Edit) Message
                await telegramService.UpdateMessageAsync(targetPeer, sentMsg.id, "Hello, this is an edited message!");
                Console.WriteLine("Message edited.");

                // Delete Message
                await telegramService.DeleteMessagesAsync(targetPeer, new[] { sentMsg.id });
                Console.WriteLine("Message deleted.");

                // 4. Media CRUD
                Console.WriteLine("\n=== 4. Media CRUD ===");
                
                // Create a temporary text file in memory to upload
                using var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello World File Content"));
                var mediaMsg = await telegramService.SendMediaAsync(targetPeer, uploadStream, "hello.txt", "Check out this file!");
                Console.WriteLine($"Media sent with ID: {mediaMsg.id}");

                // Download the media back
                using var downloadStream = await telegramService.DownloadMediaAsync(mediaMsg);
                using var reader = new StreamReader(downloadStream);
                var downloadedText = await reader.ReadToEndAsync();
                Console.WriteLine($"Downloaded media content: {downloadedText}");

                // 5. Chats & Group Management
                Console.WriteLine("\n=== 5. Chats & Group Management ===");
                
                // Create a new Chat (Groups require at least one other user besides you to be created)
                if (someUser != null)
                {
                    try
                    {
                        var chatResult = await telegramService.CreateChatAsync("My Test Group", new InputUser[] { someUser });
                        Console.WriteLine("Created Chat successfully.");
                        // To get the Chat object, you would inspect chatResult.updates 
                        // or call telegramService.GetAllDialogs() to find the newly created group.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not create chat (likely because you can't add random users to groups): {ex.Message}");
                    }
                }

                Console.WriteLine("\nAll examples completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
