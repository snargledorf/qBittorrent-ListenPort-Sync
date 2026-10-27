using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PipeCommunication;

namespace qBittorentDaemon;

public class Worker(
    IPipeClient pipeClient,
    IQBittorrentService qBittorrentService,
    IQBittorrentApiClient qBittorrentApiClient,
    INewPortNotificationService newPortNotificationService,
    IOptions<QBittorrentOptions> options,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (int port in pipeClient.MonitorPipeAsync(stoppingToken))
            {
                logger.LogInformation("Received port: {Port}", port);
                newPortNotificationService.NotifyNewPort(port);

                if (options.Value.RestartBeforePortUpdate)
                {
                    // Sometimes when the network goes out for a long time, qBittorrent may not reconnect.
                    // Restarting the application resolves this.
                    try
                    {
                        await qBittorrentApiClient.LoginAsync(stoppingToken).ConfigureAwait(false);
                        await qBittorrentApiClient.ShutdownAsync(stoppingToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to login/shutdown qBittorrent");
                    }

                    await EnsureQBittorrentFullyStoppedAsync(stoppingToken).ConfigureAwait(false);

                    await qBittorrentService.StartQBittorrentAsync(stoppingToken).ConfigureAwait(false);
                }
                
                await qBittorrentApiClient.LoginAsync(stoppingToken).ConfigureAwait(false);
                await qBittorrentApiClient.UpdatePortAsync(port, stoppingToken).ConfigureAwait(false);
                await qBittorrentApiClient.LogoutAsync(stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task EnsureQBittorrentFullyStoppedAsync(CancellationToken stoppingToken)
    {
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await qBittorrentService.WaitForExitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await qBittorrentService.KillQBittorrentProcessesAsync(stoppingToken).ConfigureAwait(false);
        }
    }
}
