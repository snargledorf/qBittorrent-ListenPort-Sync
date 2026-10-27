using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PipeCommunication;

namespace PortNumberSync;

public class Worker(ILogger<Worker> logger, IPiaService piaService, IPipeServer pipeServer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Waiting for client...");
                    await using IPipeServerClient client = await pipeServer.AcceptClientAsync(stoppingToken).ConfigureAwait(false);
                    logger.LogInformation("Client connected.");

                    await foreach (int port in piaService.MonitorPortsAsync(stoppingToken).ConfigureAwait(false))
                    {
                        logger.LogInformation("Detected new port: {Port}", port);
                        await client.SendPortAsync(port, stoppingToken).ConfigureAwait(false);
                    }
                }
                catch (IOException ex)
                {
                    logger.LogInformation(ex, "Client disconnected");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in the worker loop");
        }
    }
}