using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Ogx.Shared.Hosting.Microservices.Handlers;

public interface IResponseExceptionHandler
{
    (int code, List<string> messages) Handle([CanBeNull] Exception ex, IHostEnvironment env);
    string GetStatusCodeDescription(int statusCode);
}