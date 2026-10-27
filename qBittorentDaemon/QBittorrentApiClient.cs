using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;

namespace qBittorentDaemon;

public class QBittorrentApiClient(HttpClient httpClient, ILogger<QBittorrentApiClient> logger, IOptions<QBittorrentOptions> options) : IQBittorrentApiClient
{
    public async Task LoginAsync(CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("username", options.Value.Username),
            new KeyValuePair<string, string>("password", options.Value.Password)
        ]);
        
        await httpClient
            .PostAsync("auth/login", content, cancellationToken)
            .ConfigureAwait(false);
    }
    
    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        await httpClient.PostAsync("auth/logout", null, cancellationToken).ConfigureAwait(false);
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken)
    {
        await httpClient.PostAsync("app/shutdown", null, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdatePortAsync(int port, CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("json", $"{{\"listen_port\":{port}}}")
        ]);
        
        await httpClient
            .PostAsync("app/setPreferences", content, cancellationToken)
            .ConfigureAwait(false);
    }
}