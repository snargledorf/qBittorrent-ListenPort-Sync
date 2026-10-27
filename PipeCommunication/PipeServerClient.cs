using System.IO.Pipes;
using Microsoft.Extensions.Logging;

namespace PipeCommunication;

internal class PipeServerClient : IPipeServerClient
{
    private readonly PipeStream _serverStream;
    private readonly StreamWriter _writer;
    private readonly ILogger<PipeServerClient> _logger;

    public PipeServerClient(PipeStream serverStream, ILogger<PipeServerClient> logger)
    {
        _serverStream = serverStream;
        _writer = new StreamWriter(_serverStream);
        _logger = logger;
    }

    public async Task SendPortAsync(int port, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending port: {Port}", port);
        await _writer.WriteLineAsync(port.ToString()).ConfigureAwait(false);
        await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Port sent.");
    }

    public void Dispose()
    {
        _logger.LogDebug("Disposing pipe client...");
        _writer.Dispose();
        _serverStream.Close();
        _serverStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("Disposing pipe client...");
        await _writer.DisposeAsync();
        _serverStream.Close();
        await _serverStream.DisposeAsync();
    }
}