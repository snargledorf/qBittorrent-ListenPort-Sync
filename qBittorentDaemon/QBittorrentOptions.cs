namespace qBittorentDaemon;

public class QBittorrentOptions
{
    public const string SectionName = "QBittorrent";

    public string Path { get; set; } = @"C:\Program Files\qBittorrent\qbittorrent.exe";
    public bool RestartBeforePortUpdate { get; set; } = true;
    public string ApiUrl { get; set; } = "http://localhost:8080/api/v2/";
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = string.Empty;
}
