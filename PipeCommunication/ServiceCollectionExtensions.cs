using Microsoft.Extensions.DependencyInjection;

namespace PipeCommunication;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddPipeServer(string pipeName)
        {
            serviceCollection.AddOptions<PipeServerOptions>()
                .Configure(options => options.PipeName = pipeName);
            serviceCollection.AddSingleton<IPipeServer, PipeServer>();
            return serviceCollection;
        }

        public IServiceCollection AddPipeClient(string pipeName)
        {
            return serviceCollection.AddPipeClient(".", pipeName);
        }

        public IServiceCollection AddPipeClient(string serverName, string pipeName)
        {
            serviceCollection.AddOptions<PipeClientOptions>()
                .Configure(options =>
                {
                    options.ServerName = serverName;
                    options.PipeName = pipeName;
                });
            
            serviceCollection.AddSingleton<IPipeClient, PipeClient>();
            return serviceCollection;
        }
    }
}