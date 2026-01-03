namespace Ogx.Shared.Hosting.Microservices;

public sealed class MicroserviceHostingSettings : HostingSettings
{
    public bool IsActiveResponseDataManipulation { get; set; }

    public bool IgnoreNullValueForJsonResponse { get; set; }

    public int CachePermissionsUpdateSeconds { get; set; } = 300;
}