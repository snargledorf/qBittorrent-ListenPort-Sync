namespace PortNumberSync;

public interface IPiaService
{
    IAsyncEnumerable<int> MonitorPortsAsync(CancellationToken token);
}