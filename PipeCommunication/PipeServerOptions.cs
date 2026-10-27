namespace PipeCommunication;

internal class PipeServerOptions
{
    public string PipeName { get; set; }
}
internal class PipeClientOptions
{
    public string PipeName { get; set; }
    public string ServerName { get; set; } = ".";
}