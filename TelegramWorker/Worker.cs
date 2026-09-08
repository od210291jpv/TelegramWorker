using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TelegramClient.Services.TelegramService;
using TL;

namespace TelegramWorker;

public class Worker(ILogger<Worker> logger, ITelegramService telegram) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        

        while (!stoppingToken.IsCancellationRequested)
        {
            var dialogsResult = await telegram.GetAllChatsAsync();

            var targetDialog = dialogsResult.Dialogs.FirstOrDefault(d => dialogsResult.UserOrChat(d.Peer) is ChatBase cb && cb.ID == 2093350109);

            if(targetDialog != null)
            {
                ChatBase knownChat = (ChatBase)dialogsResult.UserOrChat(targetDialog.Peer);
                InputPeer peer = knownChat;
                ChatBase detailedChat = await telegram.GetChatAsync(peer);

                var history = await telegram.GetChatHistoryAsync(peer, limit: 20);

                foreach (Message msg in history.Messages.OfType<Message>())
                {
                    //msg.media.
                }
            }

            //var tgchat1 = await telegram.GetChatAsync(new TL.InputPeerUser(2093350109, 6), stoppingToken);
        }
    }

    private bool IsMessageContainsPhotoMedia(Message msg) 
    {
        // 1. Check for Media Presence
        if (msg.media is null)
        {
            Console.WriteLine($"[Text Only]: {msg.message}");
            return false;
        }

        if (msg.media is MessageMediaPhoto) 
        {
            return true;
        }

        return false;

        //// 2. Check the Media Type
        //switch (msg.media)
        //{
        //    case MessageMediaPhoto photoMedia:
        //        Console.WriteLine($"📸 [Photo] (ID: {photoMedia.photo.ID}) - {msg.message}");
        //        break;
        //    case MessageMediaDocument docMedia:
        //        if (docMedia.document is Document document)
        //        {
        //            // We must check the document's attributes to see what kind of file it is

        //            if (document.attributes.OfType<DocumentAttributeVideo>().Any())
        //            {
        //                Console.WriteLine($"🎥 [Video] - {msg.message}");
        //            }
        //            else if (document.attributes.OfType<DocumentAttributeAudio>().Any(a => a.flags.HasFlag(DocumentAttributeAudio.Flags.voice)))
        //            {
        //                Console.WriteLine($"🎤 [Voice Message] - {msg.message}");
        //            }
        //            else if (document.attributes.OfType<DocumentAttributeAudio>().Any())
        //            {
        //                Console.WriteLine($"🎵 [Music/Audio File] - {msg.message}");
        //            }
        //            else if (document.attributes.OfType<DocumentAttributeSticker>().Any())
        //            {
        //                Console.WriteLine($"🏷️ [Sticker] - {msg.message}");
        //            }
        //            else
        //            {
        //                Console.WriteLine($"📁 [Standard File/Document] ({document.mime_type}) - {msg.message}");
        //            }
        //        }
        //        break;
        //    case MessageMediaWebPage webMedia:
        //        Console.WriteLine($"🔗 [Link Preview] - {msg.message}");
        //        break;
        //    case MessageMediaGeo geoMedia:
        //        Console.WriteLine($"📍 [Location Pin] - {msg.message}");
        //        break;
        //    case MessageMediaPoll pollMedia:
        //        Console.WriteLine($"📊 [Poll] - {msg.message}");
        //        break;
        //    default:
        //        Console.WriteLine($"❓ [Other Media Type: {msg.media.GetType().Name}] - {msg.message}");
        //        break;
        }
}

