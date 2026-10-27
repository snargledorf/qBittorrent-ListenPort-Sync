namespace qBittorentDaemon;

public interface INewPortNotificationEvent
{
    event EventHandler<int> NewPort;
}

public interface INewPortNotificationService
{
    void NotifyNewPort(int port);
}