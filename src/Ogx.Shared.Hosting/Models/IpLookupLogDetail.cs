using JetBrains.Annotations;

namespace Ogx.Shared.Hosting.Models;

public sealed class IpLookupLogDetail
{
    [CanBeNull] public string status { get; set; }
    [CanBeNull] public string country { get; set; }
    [CanBeNull] public string region { get; set; }
    [CanBeNull] public string regionName { get; set; }
    [CanBeNull] public string city { get; set; }
    [CanBeNull] public string zip { get; set; }
    public double? lat { get; set; }
    public double? lon { get; set; }
    [CanBeNull] public string isp { get; set; }
    [CanBeNull] public string asname { get; set; }
    [CanBeNull] public string reverse { get; set; }
    public bool? mobile { get; set; }
    public bool? proxy { get; set; }
    public bool? hosting { get; set; }
    [CanBeNull] public string query { get; set; }
}