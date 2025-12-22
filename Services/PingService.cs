using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using vengar.Helpers;
using vengar.Interfaces;

namespace vengar.Services;

public class PingResult
{
    public string Hostname { get; set; }
    public DateTime Timestamp { get; set; }
    public long RoundtripTime { get; set; } // ms
    public bool Success { get; set; }
    public string Error { get; set; }
    public IPAddress? Address { get; set; }
}

public interface IPingService
{
    Task<PingResult> PingOnceAsync(PingRequest request, IWriter writer);

    Task StartContinuousPingAsync(PingRequest request, TimeSpan interval, Action<PingResult> onResult,
        CancellationToken token, IWriter writer);
}

public class PingRequest
{
    public string Host { get; init; } = "";
    public int TimeoutMs { get; init; } = 3000;
    public byte[]? Buffer { get; init; }
}

public class PingService : IPingService
{
    private readonly Ping _ping = new Ping();

    public async Task<PingResult> PingOnceAsync(PingRequest request, IWriter writer)
    {
        writer.Write(
            $"[PING] Request → Host={request.Host}, Timeout={request.TimeoutMs}ms, Buffer={request.Buffer?.Length ?? 0} bytes");
        IPAddress? ip = null;
        string hostname = request.Host;

        // forward DNS parsing
        if (IPAddress.TryParse(request.Host, out var parsedIp))
        {
            ip = parsedIp;
            writer.Write($"[DNS] Parsed IP address: {ip}");
        }
        else
        {
            try
            {
                writer.Write($"[DNS] Resolving hostname: {request.Host}");
                var entry = await Dns.GetHostEntryAsync(request.Host);
                ip = entry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                hostname = entry.HostName;
                foreach (var addr in entry.AddressList) writer.Write($"[DNS] → {addr.AddressFamily}: {addr}");
                if (ip == null) writer.Write("[DNS] No IPv4 address found");
            }
            catch (Exception ex)
            {
                writer.Write($"[DNS][ERROR] {ex.Message}");
                return new PingResult
                {
                    Hostname = request.Host,
                    Timestamp = DateTime.Now,
                    Success = false,
                    Error = $"DNS lookup failed: {ex.Message}"
                };
            }
        }

        if (ip == null)
        {
            writer.Write("[PING][ERROR] No IPv4 address available");
            return new PingResult
            {
                Hostname = request.Host, Timestamp = DateTime.Now, Success = false, Error = "No IPv4 address found"
            };
        }

        try
        {
            writer.Write($"[PING] Sending ICMP → {ip}");
            var reply = await _ping.SendPingAsync(ip, request.TimeoutMs, request.Buffer ?? Array.Empty<byte>());
            writer.Write(reply.Status == IPStatus.Success
                ? $"[PING] Reply from {ip}: time={reply.RoundtripTime}ms"
                : $"[PING] No reply from {ip}: status={reply.Status}");

            // reverse DNS 
            try
            {
                var reverse = await ReverseDnsHelper.ResolveAsync(ip);
                hostname = reverse;
                writer.Write($"[DNS] Reverse lookup → {hostname}");
            }
            catch
            {
                writer.Write("[DNS] Reverse lookup failed (ignored)");
            }

            return new PingResult
            {
                Hostname = hostname,
                Address = ip,
                Timestamp = DateTime.Now,
                RoundtripTime = reply.Status == IPStatus.Success ? reply.RoundtripTime : -1,
                Success = reply.Status == IPStatus.Success,
                Error = reply.Status.ToString()
            };
        }
        catch (Exception ex)
        {
            writer.Write($"[PING][ERROR] {ex.Message}");
            return new PingResult
            {
                Hostname = hostname,
                Address = ip,
                Timestamp = DateTime.Now,
                Success = false,
                Error = ex.Message
            };
        }
    }
    
    public async Task StartContinuousPingAsync(PingRequest request, TimeSpan interval, Action<PingResult> onResult,
        CancellationToken token, IWriter writer)
    {
        var stats = new PingStatistics();
        int printEvery = 10;
        writer.Write($"[PING] Continuous ping started → {request.Host}");
        try
        {
            while (!token.IsCancellationRequested)
            {
                var result = await PingOnceAsync(request, writer);
                stats.Register(result);
                onResult(result);
                if (stats.Sent % printEvery == 0)
                {
                    writer.Write(stats.Format(request.Buffer?.Length ?? 0));
                }

                await Task.Delay(interval, token);
            }
        }
        catch (TaskCanceledException)
        {
            // expected on cancellation
        }
        finally
        {
            writer.Write("[PING] Continuous ping stopped");
            writer.Write(stats.Format(request.Buffer?.Length ?? 0));
        }
    }

    public PingService()
    {
    }
}

sealed class PingStatistics
{
    public int Sent;
    public int Received;
    public long TotalRtt;
    public long MinRtt = long.MaxValue;
    public long MaxRtt = 0;

    public void Register(PingResult result)
    {
        Sent++;
        if (result.Success)
        {
            Received++;
            TotalRtt += result.RoundtripTime;
            MinRtt = Math.Min(MinRtt, result.RoundtripTime);
            MaxRtt = Math.Max(MaxRtt, result.RoundtripTime);
        }
    }

    public string Format(int bufferSize)
    {
        double lossPercent = Sent == 0 ? 0 : (double)(Sent - Received) / Sent * 100;
        long avgRtt = Received > 0 ? TotalRtt / Received : 0;
        return $"--- Ping statistics ---\n" +
               $"Packets: Sent = {Sent}, Received = {Received}, Lost = {Sent - Received} ({lossPercent:F1}% loss)\n" +
               $"RTT (ms): Min = {(MinRtt == long.MaxValue ? 0 : MinRtt)}, " + $"Avg = {avgRtt}, Max = {MaxRtt}\n" +
               $"Payload size: {bufferSize} bytes";
    }
}
