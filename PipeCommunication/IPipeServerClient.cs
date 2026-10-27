namespace PipeCommunication;

public interface IPipeServerClient : IDisposable, IAsyncDisposable
{
    Task SendPortAsync(int port, CancellationToken cancellationToken);
}