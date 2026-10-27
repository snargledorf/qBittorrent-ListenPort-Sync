using System.IO.Pipes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PipeCommunication;

internal class PipeServer(IOptions<PipeServerOptions> options, ILogger<PipeServer> logger, IServiceProvider serviceProvider) : IPipeServer
{
    public async Task<IPipeServerClient> AcceptClientAsync(CancellationToken cancellationToken)
    {
        NamedPipeServerStream serverStream = CreatePipe();
        logger.LogInformation("Waiting for connection...");
        await serverStream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Client connected.");
        
        await using AsyncServiceScope serviceScope = serviceProvider.CreateAsyncScope();
        return new PipeServerClient(serverStream,
            serviceScope.ServiceProvider.GetRequiredService<ILogger<PipeServerClient>>());
    }

    private NamedPipeServerStream CreatePipe()
    {
        return new NamedPipeServerStream(options.Value.PipeName, PipeDirection.Out);
    }
}