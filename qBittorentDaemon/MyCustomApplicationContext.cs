using System.Reflection;

namespace qBittorentDaemon;

public class MyCustomApplicationContext : ApplicationContext
{
    // Initialize Tray Icon
    private readonly NotifyIcon _trayIcon;

    public MyCustomApplicationContext(INewPortNotificationEvent newPortNotificationEvent)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        
        string iconName = executingAssembly.GetManifestResourceNames()
            .Single(file => file.EndsWith("qbittorrent-tray.ico"));
        
        Stream? iconStream = executingAssembly.GetManifestResourceStream(iconName);

        _trayIcon = new NotifyIcon
        {
            Icon = new Icon(iconStream),
            Visible = true,
            Text = "qBittorent Daemon",
            ContextMenuStrip = new ContextMenuStrip()
            {
                Items =
                {
                    new ToolStripMenuItem("Exit", null, Exit)
                }
            },
        };

        newPortNotificationEvent.NewPort += (sender, port) =>
        {
            _trayIcon.BalloonTipTitle = "New Port";
            _trayIcon.BalloonTipText = $"Switching port to {port}!";
            _trayIcon.ShowBalloonTip(TimeSpan.FromSeconds(5).Milliseconds);
        };
    }

    private void Exit(object? sender, EventArgs e)
    {
        // Hide tray icon, otherwise it will remain shown until user mouses over it
        _trayIcon.Visible = false;
        Application.Exit();
    }
}