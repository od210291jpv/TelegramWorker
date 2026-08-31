using TelegramClient.Services.TelegramService;
using TelegramWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "TelegramWorker";
});
builder.Services.AddSystemd();

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ITelegramService, TelegramService>();

var host = builder.Build();
host.Run();
