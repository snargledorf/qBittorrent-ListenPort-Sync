using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PipeCommunication;
using PortNumberSync;
using Serilog;

if (args.Length > 0)
{
    if (args[0] == "-i")
    {
        string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exePath))
        {
            Console.WriteLine("Could not determine executable path.");
            return;
        }

        Console.WriteLine($"Installing service with binary path: {exePath}");

        var psi = new ProcessStartInfo
        {
            FileName = "sc.exe",
            Arguments = $"create PortNumberSync binPath= \"{exePath}\" start=auto",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using Process? process = Process.Start(psi);
        if (process != null)
        {
            await process.WaitForExitAsync();
            Console.WriteLine(await process.StandardOutput.ReadToEndAsync());
            Console.WriteLine(await process.StandardError.ReadToEndAsync());

            if (process.ExitCode == 0)
            {
                Console.WriteLine("Service installed successfully.");
            }
        }
        return;
    }

    if (args[0] == "-u")
    {
        Console.WriteLine("Uninstalling service...");

        var psi = new ProcessStartInfo
        {
            FileName = "sc.exe",
            Arguments = "delete PortNumberSync",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using Process? process = Process.Start(psi);
        if (process != null)
        {
            await process.WaitForExitAsync();
            Console.WriteLine(await process.StandardOutput.ReadToEndAsync());
            Console.WriteLine(await process.StandardError.ReadToEndAsync());

            if (process.ExitCode == 0)
            {
                Console.WriteLine("Service uninstalled successfully.");
            }
        }
        return;
    }
}

string? cwd = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
Directory.SetCurrentDirectory(cwd ?? ".");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        
builder.Services.AddLogging(logging => logging.AddSerilog(new LoggerConfiguration()
    .WriteTo.File("portmonitor-log.txt")
    .CreateLogger()));

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "PortNumberSync";
});

builder.Services.AddPipeServer("piaPortMonitorServicePipe");
builder.Services.AddSingleton<IPiaService, PiaService>();
builder.Services.AddHostedService<Worker>();

IHost app = builder.Build();

await app.RunAsync();