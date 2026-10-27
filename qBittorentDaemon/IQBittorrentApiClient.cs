namespace qBittorentDaemon;

public interface IQBittorrentApiClient
{
    Task LoginAsync(CancellationToken cancellationToken);
    Task LogoutAsync(CancellationToken cancellationToken);
    Task ShutdownAsync(CancellationToken cancellationToken);
    Task UpdatePortAsync(int port, CancellationToken cancellationToken);
}