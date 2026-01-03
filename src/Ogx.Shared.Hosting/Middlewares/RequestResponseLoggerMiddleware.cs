using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Claims;
using Ogx.Shared.Hosting.Models;
using HsnSoft.Base.AspNetCore.Logging;
using HsnSoft.Base.AspNetCore.Tracing;
using HsnSoft.Base.Json.Newtonsoft.Mask;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Ogx.Shared.Hosting.Middlewares;

public sealed class RequestResponseLoggerMiddleware(IOptions<HostingSettings> settings, IRequestResponseLogger logger) : IMiddleware
{
    private readonly HostingSettings _settings = settings.Value;

    private readonly string[] _blacklist =
    [
        "password", "pwd", "clientsecret", "accesstoken", "refreshtoken",
        "*payload.password", "*payload.pwd", "*payload.clientsecret", "*payload.accesstoken", "*payload.refreshtoken"
    ];

    private const string MaskValue = "******";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_settings?.IsEnabledRequestResponseLogger == false)
        {
            await next(context);
            return;
        }

        string pathString = context.Request.Path.ToString().ToLower();
        if (pathString.Equals("/startupcheck") || pathString.Equals("/livenesscheck") || pathString.Equals("/readinesscheck"))
        {
            if (_settings?.IsEnabledHealthCheckRequestLogger == false)
            {
                await next(context);
                return;
            }
        }

        var watch = new Stopwatch();
        watch.Start();

        var reqStartTime = DateTime.UtcNow;
        var log = new RequestResponseLogModel();
        var request = context.Request;

        /*log*/
        log.LogId = Guid.NewGuid().ToString();
        log.TraceId = context.TraceIdentifier;
        log.CorrelationId = context.GetCorrelationId();
        log.Facility = nameof(RequestResponseLogFacility.HTTP_REQUEST_LOG);


        var ip = request.HttpContext.Connection.RemoteIpAddress;
        log.ClientInfo = new ClientInfoLogDetail
        {
            LocalIp = ip?.ToString(),
            ClientLat = context.GetClientRequestLat(),
            ClientLong = context.GetClientRequestLong(),
            ClientVersion = context.GetClientVersion(),
            ClientUserId = Guid.Empty.ToString(),
            ClientUserRole = "anonymous"
        };

        if (request.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedClientIp))
        {
            log.ClientInfo.ClientForwardedIp = forwardedClientIp.ToString();
        }

        if (request.HttpContext.Request.Headers.TryGetValue("Origin", out var originHost))
        {
            log.ClientInfo.ClientOrigin = originHost.ToString();
            if (!string.IsNullOrWhiteSpace(log.ClientInfo.ClientOrigin))
            {
                log.ClientInfo.ClientOrigin = log.ClientInfo.ClientOrigin.ToLower()
                    .Replace("http://", "")
                    .Replace("https://", "")
                    .Replace("www.", "")
                    .Replace("null", "")
                    .Split("/")[0];
            }
        }

        if (request.HttpContext.Request.Headers.TryGetValue("Referer", out var refererHost))
        {
            log.ClientInfo.ClientReferer = refererHost.ToString();
            if (!string.IsNullOrWhiteSpace(log.ClientInfo.ClientReferer))
            {
                log.ClientInfo.ClientReferer = log.ClientInfo.ClientReferer.ToLower()
                    .Replace("http://", "")
                    .Replace("https://", "")
                    .Replace("www.", "")
                    .Replace("null", "")
                    .Split("/")[0];
            }
        }

        if (string.IsNullOrWhiteSpace(log.ClientInfo.ClientOrigin) && !string.IsNullOrWhiteSpace(log.ClientInfo.ClientReferer))
        {
            log.ClientInfo.ClientOrigin = log.ClientInfo.ClientReferer;
        }

        if (request.HttpContext.Request.Headers.TryGetValue("From", out var fromHost))
        {
            log.ClientInfo.ClientFrom = fromHost.ToString();
            if (!string.IsNullOrWhiteSpace(log.ClientInfo.ClientFrom))
            {
                log.ClientInfo.ClientFrom = log.ClientInfo.ClientFrom.ToLower()
                    .Replace("http://", "")
                    .Replace("https://", "")
                    .Replace("www.", "")
                    .Replace("null", "")
                    .Split("/")[0];
            }
        }

        if (request.HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
        {
            log.ClientInfo.ClientUserAgent = UserAgentProvider.GetUserAgentDetails(userAgent.ToString());
        }

        if (request.HttpContext.Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage))
        {
            log.ClientInfo.ClientLanguage = acceptLanguage.ToString();
        }

        /*request*/
        log.RequestInfo = new RequestInfoLogDetail
        {
            RequestDateTimeUtc = reqStartTime,
            RequestMethod = request.Method,
            RequestPath = request.Path,
            RequestBody = await ReadBodyFromRequest(request),
            RequestScheme = request.Scheme,
            RequestHost = request.Host.ToString()
        };
        string requestQuery = request.QueryString.ToString();
        var requestHeaders = FormatHeaders(request.Headers);

        // Temporarily replace the HttpResponseStream,
        // which is a write-only stream, with a MemoryStream to capture
        // its value in-flight.
        var response = context.Response;
        var originalResponseBody = response.Body;
        var newResponseBody = new MemoryStream();
        response.Body = newResponseBody;

        // _logger.RequestResponseInfoLog(log);

        try
        {
            // Call the next middleware in the pipeline
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception.Message);
        }

        newResponseBody.Seek(0, SeekOrigin.Begin);
        string responseBodyText = await new StreamReader(newResponseBody).ReadToEndAsync();
        if (!string.IsNullOrWhiteSpace(responseBodyText) && response.Headers.TryGetValue("Content-Type", out var responseContentType))
        {
            responseBodyText = responseContentType.ToString().Equals("application/json")
                ? responseBodyText.MaskFields(_blacklist, MaskValue)
                : "No JSON Response";
        }
        else
        {
            responseBodyText = "No JSON Response";
        }

        newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBody);
        await newResponseBody.DisposeAsync();

        watch.Stop();
        long elapsedMilliseconds = watch.ElapsedMilliseconds;
        SetSessionUserInfo(request.HttpContext.User, ref log);

        /*response*/
        var responseHeader = FormatHeaders(response.Headers);
        log.ResponseInfo = new ResponseInfoLogDetail { ResponseStatus = response.StatusCode.ToString(), ResponseBody = responseBodyText, ResponseDateTimeUtc = DateTime.UtcNow };

        log.RequestResponseWorkingTime = $"{elapsedMilliseconds:0.####}ms";

        if (response?.StatusCode >= 400)
        {
            log.Facility = nameof(RequestResponseLogFacility.HTTP_REQUEST_ERROR_LOG);
            log.RequestInfo.RequestHeaders = requestHeaders;
            log.RequestInfo.RequestQuery = requestQuery;
            log.ResponseInfo.ResponseHeaders = responseHeader;

            if (!string.IsNullOrWhiteSpace(log.ClientInfo.ClientForwardedIp))
            {
                log.ClientInfo.ClientForwardedDetails = await GetIpDetails(log.ClientInfo.ClientForwardedIp);
            }

            logger.RequestResponseErrorLog(log);
        }
        else
        {
            log.Facility = nameof(RequestResponseLogFacility.HTTP_REQUEST_RESPONSE_LOG);

            logger.RequestResponseInfoLog(log);
        }
    }

    private Dictionary<string, string> FormatHeaders(IHeaderDictionary headers)
    {
        var pairs = new Dictionary<string, string>();
        foreach (var header in headers)
        {
            if (header.Key.Equals("Authorization") && header.Value.ToString().StartsWith("Bearer "))
            {
                pairs.Add(header.Key, "Bearer --AccessToken--");
                continue;
            }

            pairs.Add(header.Key, header.Value);
        }

        return pairs;
    }

    private List<KeyValuePair<string, string>> FormatQueries(string queryString)
    {
        var pairs = new List<KeyValuePair<string, string>>();
        foreach (string query in queryString.TrimStart('?').Split("&"))
        {
            string[] items = query.Split("=");
            string key = items.Any() ? items[0] : string.Empty;
            string value = items.Length >= 2 ? items[1] : string.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                pairs.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        return pairs;
    }

    private async Task<string> ReadBodyFromRequest(HttpRequest request)
    {
        if (request.ContentLength is null or 0 || request.ContentType?.Contains("application/json") != true)
        {
            return string.Empty;
        }

        try
        {
            // Ensure the request's body can be read multiple times
            // (for the next middlewares in the pipeline).
            request.EnableBuffering();
            using var streamReader = new StreamReader(request.Body, leaveOpen: true);
            string requestBody = await streamReader.ReadToEndAsync();
            // Reset the request's body stream position for
            // next middleware in the pipeline.
            request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(requestBody) && (requestBody.StartsWith("{") || requestBody.StartsWith("[")) && (requestBody.EndsWith("}") || requestBody.EndsWith("]")))
            {
                return requestBody.MaskFields(_blacklist, MaskValue);
            }

            return requestBody;
        }
        catch (IOException)
        {
            return string.Empty;
        }
    }

    private async Task<IpLookupLogDetail> GetIpDetails(string ipAddress)
    {
        try
        {
            string route = $"http://ip-api.com/json/{ipAddress}?fields=21230333";
            return await new HttpClient().GetFromJsonAsync<IpLookupLogDetail>(route);
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }

    private static void SetSessionUserInfo(ClaimsPrincipal principal, ref RequestResponseLogModel log)
    {
        var userIdOrNull = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdOrNull != null && !userIdOrNull.Value.IsNullOrWhiteSpace())
        {
            log.ClientInfo.ClientUserId = userIdOrNull.Value;
        }

        var roles = principal.Claims.Where(c => c.Type == ClaimTypes.Role).ToArray();
        if (roles is { Length: > 0 })
        {
            log.ClientInfo.ClientUserRole = roles.Select(c => c.Value).Distinct().ToArray().JoinAsString(",");
        }
    }
}