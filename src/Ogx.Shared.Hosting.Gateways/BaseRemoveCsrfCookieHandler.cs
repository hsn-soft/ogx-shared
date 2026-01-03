using HsnSoft.Base.AspNetCore.Mvc.AntiForgery;
using Microsoft.Extensions.Options;

namespace Ogx.Shared.Hosting.Gateways;

public class BaseRemoveCsrfCookieHandler : DelegatingHandler
{
    private const string CookieHeaderName = "Cookie";
    private readonly BaseAntiForgeryOptions _baseAntiForgeryOptions;

    public BaseRemoveCsrfCookieHandler(IOptions<BaseAntiForgeryOptions> baseAntiForgeryOptions)
    {
        _baseAntiForgeryOptions = baseAntiForgeryOptions.Value;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string authCookieName = _baseAntiForgeryOptions.AuthCookieSchemaName;
        string antiForgeryCookieName = _baseAntiForgeryOptions.TokenCookie.Name;

        if (request.Headers.TryGetValues(CookieHeaderName, out var cookies))
        {
            var newCookies = cookies.ToList();

            newCookies.RemoveAll(x =>
                !string.IsNullOrWhiteSpace(authCookieName) && x.Contains(authCookieName) ||
                !string.IsNullOrWhiteSpace(antiForgeryCookieName) && x.Contains(antiForgeryCookieName));

            request.Headers.Remove(CookieHeaderName);
            request.Headers.Add(CookieHeaderName, newCookies);
        }

        return base.SendAsync(request, cancellationToken);
    }
}