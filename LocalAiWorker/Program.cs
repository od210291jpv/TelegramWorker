using LocalAiWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "TelegramWorker";
});
builder.Services.AddSystemd();

var host = builder.Build();
host.Run();
