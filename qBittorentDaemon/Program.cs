using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PipeCommunication;
using qBittorentDaemon;
using Serilog;
using WindowsFormsLifetime;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.UseWindowsFormsLifetime<MyCustomApplicationContext>();

builder.Services.AddLogging(logging => logging.AddSerilog(new LoggerConfiguration()
    .WriteTo.File("daemon-log.txt")
    .CreateLogger()));
        
builder.Services.Configure<QBittorrentOptions>(
    builder.Configuration.GetSection(QBittorrentOptions.SectionName));

builder.Services.AddPipeClient("piaPortMonitorServicePipe");
        
builder.Services.AddHttpClient<IQBittorrentApiClient, QBittorrentApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<QBittorrentOptions>>().Value;
    client.BaseAddress = new Uri(options.ApiUrl);
});

builder.Services.AddSingleton<IQBittorrentService, QBittorrentService>();

builder.Services.AddSingleton<NewPortNotificationService>();
builder.Services.AddSingleton<INewPortNotificationService>(sp =>
    sp.GetRequiredService<NewPortNotificationService>());
builder.Services.AddSingleton<INewPortNotificationEvent>(sp =>
    sp.GetRequiredService<NewPortNotificationService>());
        
builder.Services.AddHostedService<Worker>();

IHost app = builder.Build();

await app.RunAsync();