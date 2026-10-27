namespace PipeCommunication;

public interface IPipeClient
{
    IAsyncEnumerable<int> MonitorPipeAsync(CancellationToken cancellationToken);
}