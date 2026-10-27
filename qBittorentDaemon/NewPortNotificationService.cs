using Microsoft.Extensions.Logging;

namespace qBittorentDaemon;

public class NewPortNotificationService(ILogger<NewPortNotificationService> logger) : INewPortNotificationService, INewPortNotificationEvent
{
    public event EventHandler<int>? NewPort;
    
    public void NotifyNewPort(int port) => NewPort?.Invoke(this, port);
}