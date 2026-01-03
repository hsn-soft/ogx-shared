using System.Globalization;
using UAParser;

namespace Ogx.Shared.Hosting.Models;

public static class UserAgentProvider
{
    public static ClientUserAgentDetail GetUserAgentDetails(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return null;

        try
        {
            var result = new ClientUserAgentDetail { ua = userAgent };

            var parser = Parser.GetDefault();
            var clientInfo = parser.Parse(userAgent);

            result.browser = clientInfo.UA.Family; // + " " + clientInfo.UA.Major;
            result.os = clientInfo.OS.Family;
            result.deviceFamily = string.IsNullOrEmpty(clientInfo.Device.Family) ? "Unknown" : clientInfo.Device.Family;
            result.engine = DetectEngine(userAgent);
            result.deviceType = GetDeviceType(clientInfo, userAgent);

            return result;
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }

    private static string DetectEngine(string ua)
    {
        string tmpUa = ua?.ToLower(new CultureInfo("en-US")) ?? string.Empty;
        if (tmpUa.Contains("postman")) return "Postman";
        if (tmpUa.Contains("curl")) return "Curl";
        if (tmpUa.Contains("browsehere")) return "BrowseHere";

        if (tmpUa.Contains("trident")) return "Trident";
        if (tmpUa.Contains("gecko") && !tmpUa.Contains("khtml")) return "Gecko";
        if (tmpUa.Contains("applewebkit"))
        {
            if (tmpUa.Contains("chrome") || tmpUa.Contains("chromium") || tmpUa.Contains("edg") || tmpUa.Contains("opr"))
                return "Blink";
            return "WebKit";
        }

        return "Unknown";
    }

    private static string GetDeviceType(ClientInfo client, string ua)
    {
        string deviceFamily = client.Device.Family?.ToLower(new CultureInfo("en-US")) ?? string.Empty;
        string osFamily = client.OS.Family?.ToLower(new CultureInfo("en-US")) ?? string.Empty;
        string tmpUa = ua?.ToLower(new CultureInfo("en-US")) ?? string.Empty;

        // Application client keywords
        string[] appKeywords = ["postman", "curl", "httpclient", "python-requests", "okhttp", "java", "wget", "bot"];

        // Console keywords
        string[] consoleKeywords = ["playstation", "xbox", "nintendo", "switch", "steam"];

        // Common TV keywords (Smart TV platforms & models)
        string[] tvKeywords =
        [
            "browsehere", "smarttv", "smart-tv", "hbbtv", "netcast", "webos", "tizen", "bravia", "appletv", "roku", "googletv", "firetv", "aftmm", "aftss", "aftt", "shieldtv", "mi tv", "philips tv", "hisense", "panasonic tv"
        ];

        // Common Tablet keywords
        string[] tabletKeywords =
        [
            "ipad", "tablet", "sm-t", "sm-x", "galaxy tab", "huawei mediapad",
            "web-w09", "agr-w09", "lenovo tab", "xiaomi pad", "mi pad", "nexus 7", "nexus 9",
            "amazon kindle", "fire hd", "zte k", "mediapad", "m2101k9g", "redmi pad",
            "oneplus pad", "hmscore"
        ];

        // --- Application detection ---
        if (appKeywords.Any(keyword => tmpUa.Contains(keyword)))
            return "Application";

        // --- Console detection ---
        if (consoleKeywords.Any(keyword => tmpUa.Contains(keyword)))
        {
            return "Console";
        }

        // --- TV detection ---
        if (tvKeywords.Any(keyword => tmpUa.Contains(keyword)))
            return "TV";

        if (tmpUa.Contains("android") && tmpUa.Contains("tv"))
            return "TV";

        // --- Tablet detection ---
        if (tabletKeywords.Any(keyword => tmpUa.Contains(keyword)) ||
            deviceFamily.Contains("ipad") ||
            deviceFamily.Contains("tablet"))
            return "Tablet";

        if (deviceFamily.Contains("android") && !tmpUa.Contains("mobile") && !tmpUa.Contains("tv"))
            return "Tablet";

        // Mobile detection
        if (deviceFamily.Contains("iphone")
            || (deviceFamily.Contains("android") && tmpUa.Contains("mobile"))
            || (!osFamily.Contains("mac os") && tmpUa.Contains("mobile"))
            || osFamily.Contains("symbian"))
        {
            return "Mobile";
        }

        // --- Extra Android fallback ---
        if (tmpUa.Contains("android") && !tmpUa.Contains("mobile") && !tmpUa.Contains("tablet") && !tmpUa.Contains("tv") && !tmpUa.Contains("x11"))
            return "Mobile";

        // Desktop detection
        if (osFamily.Contains("windows")
            || osFamily.Contains("mac os")
            || osFamily.Contains("linux")
            || osFamily.Contains("chrome os")
            || tmpUa.Contains("x11"))
        {
            return "Desktop";
        }

        return "Unknown";
    }
}