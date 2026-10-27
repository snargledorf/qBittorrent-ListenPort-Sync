namespace qBittorentDaemon;

public interface IQBittorrentService
{
    Task KillQBittorrentProcessesAsync(CancellationToken token);
    Task StartQBittorrentAsync(CancellationToken token);
    Task WaitForExitAsync(CancellationToken token);
}