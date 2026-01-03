namespace Ogx.Shared.Contracts.Cache;

public interface IServicePermissionProvider
{
    Task<List<string>> GetServicePermissionKeysAsync();
}