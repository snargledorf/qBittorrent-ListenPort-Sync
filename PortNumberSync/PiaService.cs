using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace PortNumberSync;

public class PiaService(ILogger<PiaService> logger) : IPiaService
{
    private static readonly ProcessStartInfo PiaProcessStartInfo = new()
    {
        FileName = @"C:\Program Files\Private Internet Access\piactl.exe",
        Arguments = "monitor portforward",
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = false,
    };
    
    public async IAsyncEnumerable<int> MonitorPortsAsync([EnumeratorCancellation] CancellationToken token)
    {
        logger.LogInformation("Starting PIA monitor using: {PiaCtlPath}", PiaProcessStartInfo.FileName);

        while (!token.IsCancellationRequested)
        {
            using Process process = StartPiaCtlProcess();
            try
            {
                StreamReader stdout = process.StandardOutput;

                while (!token.IsCancellationRequested &&
                       await stdout.ReadLineAsync(token).ConfigureAwait(false) is { } line)
                {
                    logger.LogInformation("Received line: {line}", line);
                    
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (!IsIntegerPortLine(line, out int port))
                        continue;
                    
                    if (port is < 1 or > 65535)
                        continue;

                    logger.LogInformation("Received port: {port}", port);
                    yield return port;
                }

                if (token.IsCancellationRequested)
                    break;

                logger.LogInformation("piactl process ended; restarting in 2s...");
                await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
            }
            finally
            {
                process.Kill();
                await process.WaitForExitAsync(token).ConfigureAwait(false);
            }
        }
    }

    private static Process StartPiaCtlProcess()
    {
        return Process.Start(PiaProcessStartInfo) ??
               throw new InvalidOperationException("Failed to start piactl process.");
    }

    private static bool IsIntegerPortLine(string line, out int port)
    {
        return int.TryParse(line, out port);
    }
}