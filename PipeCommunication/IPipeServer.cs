namespace PipeCommunication;

public interface IPipeServer
{
    Task<IPipeServerClient> AcceptClientAsync(CancellationToken cancellationToken);
}