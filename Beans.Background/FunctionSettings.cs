namespace Beans.Background;
public sealed class FunctionSettings
{
    public string ApiBase { get; set; }
    public string ApiKey { get; set; }
    public string ContainerName { get; set; }
    public string LogConnectionString { get; set; }
    public string LogFilenameBase { get; set; }

    public FunctionSettings()
    {
        ApiBase = string.Empty;
        ApiKey = string.Empty;
        ContainerName = string.Empty;
        LogConnectionString = string.Empty;
        LogFilenameBase = string.Empty;
    }
}
