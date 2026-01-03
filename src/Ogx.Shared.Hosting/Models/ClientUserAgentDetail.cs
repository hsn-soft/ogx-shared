using JetBrains.Annotations;

namespace Ogx.Shared.Hosting.Models;

public sealed class ClientUserAgentDetail
{
    [CanBeNull] public string ua { get; set; }
    [CanBeNull] public string browser { get; set; }
    [CanBeNull] public string os { get; set; }
    [CanBeNull] public string deviceFamily { get; set; }
    [CanBeNull] public string engine { get; set; }
    [CanBeNull] public string deviceType { get; set; }
}