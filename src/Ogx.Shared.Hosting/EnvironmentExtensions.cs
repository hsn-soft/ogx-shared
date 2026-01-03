using Microsoft.Extensions.Hosting;

namespace Ogx.Shared.Hosting;

public static class EnvironmentExtensions
{
    public static bool IsHostProduction(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null) throw new ArgumentNullException(nameof(hostEnvironment));

        return hostEnvironment.IsEnvironment("stage") || hostEnvironment.IsEnvironment("production");
    }

    public static bool IsIntegrationTest(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null) throw new ArgumentNullException(nameof(hostEnvironment));

        return hostEnvironment.IsEnvironment(EnvironmentNames.IntegrationTestEnvironment);
    }
}

public static class EnvironmentNames
{
    public const string IntegrationTestEnvironment = nameof(IntegrationTestEnvironment);
}