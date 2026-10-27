using System.IO.Pipes;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PipeCommunication;

internal class PipeClient(IOptions<PipeClientOptions> options, ILogger<PipeClient> logger) : IPipeClient
{
    public async IAsyncEnumerable<int> MonitorPipeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var clientStream = new NamedPipeClientStream(options.Value.ServerName, options.Value.PipeName, PipeDirection.In);
        logger.LogInformation("Connecting to pipe...");
        await clientStream.ConnectAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Connected to pipe.");
        
        using var reader = new StreamReader(clientStream);
        
        while (!cancellationToken.IsCancellationRequested && clientStream.IsConnected)
        {
            string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            logger.LogDebug("Received line: {Line}", line);
            if (int.TryParse(line, out int port))
            {
                logger.LogInformation("Received port: {Port}", port);
                yield return port;
            }
        }

        logger.LogInformation("Read loop exited, closing pipe. {cancelled} {isConnected}",
            cancellationToken.IsCancellationRequested ? "cancelled" : "not cancelled",
            clientStream.IsConnected ? "connected" : "disconnected");
        
        clientStream.Close();
        logger.LogInformation("Pipe connection closed.");
    }
}