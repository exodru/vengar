using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace vengar.Helpers;

public static class ReverseDnsHelper
{
    private static readonly ConcurrentDictionary<IPAddress, string> _cache = new();

    /// <summary>
    /// Performs a cached reverse DNS (PTR) lookup.
    /// Returns hostname if found, otherwise returns the IP string.
    /// </summary>
    public static async Task<string> ResolveAsync(
        IPAddress ip,
        int timeoutMs = 1500)
    {
        if (_cache.TryGetValue(ip, out var cached))
            return cached;

        try
        {
            using var cts = new CancellationTokenSource(timeoutMs);

            var entry = await Dns
                .GetHostEntryAsync(ip)
                .WaitAsync(cts.Token);

            var hostname = entry.HostName;
            _cache[ip] = hostname;
            return hostname;
        }
        catch
        {
            var fallback = ip.ToString();
            _cache[ip] = fallback;
            return fallback;
        }
    }

    /// Clears the reverse DNS cache (optional).
    public static void ClearCache()
    {
        _cache.Clear();
    }
}
