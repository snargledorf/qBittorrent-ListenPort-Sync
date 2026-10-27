using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace qBittorentDaemon;

public class QBittorrentService(IOptions<QBittorrentOptions> options, ILogger<QBittorrentService> logger) : IQBittorrentService
{
    public Task KillQBittorrentProcessesAsync(CancellationToken token)
    {
        logger.LogInformation("Killing qBittorrent processes...");
        Process[] qBittorrentProcesses = Process.GetProcessesByName("qbittorrent");
        foreach (Process qBittorrentProcess in qBittorrentProcesses)
        {
            logger.LogInformation("Killing qBittorrent process {pid}", qBittorrentProcess.Id);
            qBittorrentProcess.Kill();
        }

        return Task.CompletedTask;
    }

    public Task StartQBittorrentAsync(CancellationToken token)
    {
        var psi = new ProcessStartInfo
        {
            FileName = options.Value.Path,
            UseShellExecute = false
        };

        logger.LogInformation("Launching qBittorrent {psi}", psi);
        using Process process = Process.Start(psi) ??
                                throw new InvalidOperationException("Failed to start qbittorrent process.");
        
        return Task.CompletedTask;
    }

    public async Task WaitForExitAsync(CancellationToken token)
    {
        Process[] qBittorrentProcesses = Process.GetProcessesByName("qbittorrent");
        await Task.WhenAll(qBittorrentProcesses.Select(p => p.WaitForExitAsync(token))).ConfigureAwait(false);
    }
}