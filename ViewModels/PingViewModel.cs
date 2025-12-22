using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using vengar.Data;
using vengar.Helpers;
using vengar.Interfaces;
using vengar.Services;

namespace vengar.ViewModels;

public partial class PingViewModel : PageViewModel
{
    private IWriter _writer;
    private readonly IPingService _pingService;
    [ObservableProperty] private string? _pingReply;
    [ObservableProperty] private string? _ipAddressField;
    [ObservableProperty] private string _timeoutField = "3000";
    [ObservableProperty] private string _bufferSizeField = "0";
    [ObservableProperty] private bool _isContinuousPinging;
    [ObservableProperty] private string _intervalField = "1000"; // ms
    private CancellationTokenSource? _continuousPingCts;
    public bool IsNotContinuousPinging => !IsContinuousPinging;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(PingAddressCommand))]
    private ObservableCollection<IPing> _previousAddresses = new() { };

    private CancellationTokenSource? _sweepCts;
    [ObservableProperty] private bool _isSweeping;
    public bool IsNotSweeping => !IsSweeping;

    [RelayCommand]
    private async Task PingAddress()
    {
        if (string.IsNullOrWhiteSpace(IpAddressField))
        {
            return;
        }

        var request = BuildPingRequest();
        var result = await _pingService.PingOnceAsync(request, _writer);
        PingEntry newPing = new PingEntry();
        if (result.Success)
        {
            PingReply = $"Reply from {IpAddressField}: time={result.RoundtripTime}ms";
            newPing.Hostname = result?.Hostname;
            newPing.Address = result.Address?.ToString();
            newPing.Rtt = result.RoundtripTime;
            newPing.Failure = (result.Success == true) ? 0 : 1;
            newPing.Success = (result.Success == true) ? 1 : 0;
            newPing.BufferSize = request.Buffer!.Length;
        }
        else
        {
            PingReply = $"Ping failed: {result.Error}";
        }

        PreviousAddresses.Add(newPing);
    }

    [RelayCommand]
    private void SaveLogs(Visual visual)
    {
        _writer.ExportLogsAsync(visual, "ping-logs");
    }

    [RelayCommand]
    private void ClearLogTable()
    {
        PreviousAddresses.Clear();
    }

    [RelayCommand]
    private void ClearLogFile()
    {
        _writer.ClearLogs();
    }

    [RelayCommand(CanExecute = nameof(CanSweep))]
    private async Task PingSweep()
    {
        if (IsSweeping) return;
        if (!TryParseRange(IpAddressField!, out var addresses))
        {
            PingReply = "Invalid IP range or CIDR.";
            return;
        }

        PingReply = "Starting sweep...";
        _writer.Write($"[SWEEP]: Sweep starting...");
        IsSweeping = true;
        _sweepCts = new CancellationTokenSource();
        PreviousAddresses.Clear();
        try
        {
            var tasks = addresses.Select(ip => SweepSingle(ip, _sweepCts.Token));
            await Task.WhenAll(tasks);
            PingReply = "Sweep complete.";
            _writer.Write($"[SWEEP]: Sweep is complete.");
        }
        catch (TaskCanceledException)
        {
            PingReply = "Sweep cancelled.";
            _writer.Write($"[SWEEP]: Sweep cancelled.");
        }
        finally
        {
            IsSweeping = false;
            _sweepCts.Dispose();
            _sweepCts = null;
        }
    }

    private bool CanSweep() => !IsSweeping;

    [RelayCommand]
    private void CancelSweep()
    {
        _sweepCts?.Cancel();
    }

    private async Task SweepSingle(IPAddress ip, CancellationToken token)
    {
        using var pinger = new Ping();
        try
        {
            var reply = await pinger.SendPingAsync(ip, TimeSpan.FromMilliseconds(500), // timeout
                Array.Empty<byte>(), // buffer
                null, // PingOptions
                token // cancellation
            );

            // Reverse DNS (PTR lookup)
            var hostname = await ReverseDnsHelper.ResolveAsync(ip);

            var entry = new PingEntry
            {
                Address = ip.ToString(),
                Hostname = hostname,
                Success = reply.Status == IPStatus.Success ? 1 : 0,
                Failure = reply.Status != IPStatus.Success ? 1 : 0,
                Rtt = reply.Status == IPStatus.Success ? reply.RoundtripTime : 0,
                BufferSize = 0
            };
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() => { PreviousAddresses.Add(entry); });
        }
        catch (PingException)
        {
            // ignore unreachable hosts silently
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sweep error for {ip}: {ex.Message}");
        }
    }

    private bool TryParseRange(string input, out IEnumerable<IPAddress> addresses)
    {
        addresses = Enumerable.Empty<IPAddress>();
        input = input.Trim();

        // Range: 192.168.1.10-192.168.1.50
        if (input.Contains('-'))
        {
            var parts = input.Split('-');
            if (IPAddress.TryParse(parts[0], out var startIp) && IPAddress.TryParse(parts[1], out var endIp))
            {
                addresses = EnumerateRange(startIp, endIp);
                return true;
            }

            return false;
        }

        // CIDR: 192.168.1.0/24
        if (input.Contains('/'))
        {
            return TryParseCidr(input, out addresses);
        }

        return false;
    }

    private IEnumerable<IPAddress> EnumerateRange(IPAddress start, IPAddress end)
    {
        var s = start.GetAddressBytes();
        var e = end.GetAddressBytes();
        uint startNum = BitConverter.ToUInt32(s.Reverse().ToArray());
        uint endNum = BitConverter.ToUInt32(e.Reverse().ToArray());
        for (uint i = startNum; i <= endNum; i++)
            yield return new IPAddress(BitConverter.GetBytes(i).Reverse().ToArray());
    }

    private bool TryParseCidr(string cidr, out IEnumerable<IPAddress> addresses)
    {
        addresses = Enumerable.Empty<IPAddress>();
        var parts = cidr.Split('/');
        if (parts.Length != 2) return false;
        if (!IPAddress.TryParse(parts[0], out var baseIp)) return false;
        if (!int.TryParse(parts[1], out var maskBits)) return false;
        var ipBytes = BitConverter.ToUInt32(baseIp.GetAddressBytes().Reverse().ToArray());
        uint mask = uint.MaxValue << (32 - maskBits);
        uint network = ipBytes & mask;
        uint broadcast = network | ~mask;
        var result = new List<IPAddress>();
        for (uint i = network + 1; i < broadcast; i++)
        {
            result.Add(new IPAddress(BitConverter.GetBytes(i).Reverse().ToArray()));
        }

        addresses = result;
        return true;
    }

    private PingRequest BuildPingRequest()
    {
        if (!int.TryParse(TimeoutField, out var timeout)) timeout = 3000;
        byte[] buffer = [];
        if (int.TryParse(BufferSizeField, out var bufferSize) && bufferSize > 0)
        {
            buffer = new byte[bufferSize];
            Random.Shared.NextBytes(buffer);
        }

        return new PingRequest { Host = IpAddressField.Trim(), TimeoutMs = timeout, Buffer = buffer };
    }

    [RelayCommand(CanExecute = nameof(CanStartContinuousPing))]
    private async Task StartContinuousPing()
    {
        if (string.IsNullOrWhiteSpace(IpAddressField)) return;
        IsContinuousPinging = true;
        _continuousPingCts = new CancellationTokenSource();
        if (!int.TryParse(IntervalField, out var intervalMs)) intervalMs = 1000;
        var request = BuildPingRequest();
        int sent = 0;
        int received = 0;
        long totalRtt = 0;
        await _pingService.StartContinuousPingAsync(request, TimeSpan.FromMilliseconds(intervalMs), result =>
        {
            sent++;
            var entry = new PingEntry
            {
                Hostname = result.Hostname,
                Address = result.Address?.ToString(),
                Rtt = result.RoundtripTime,
                Success = result.Success ? 1 : 0,
                Failure = result.Success ? 0 : 1,
                BufferSize = request.Buffer?.Length ?? 0
            };
            if (result.Success)
            {
                received++;
                totalRtt += result.RoundtripTime;
            }

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                PreviousAddresses.Add(entry);
                double loss = sent == 0 ? 0 : (double)(sent - received) / sent * 100;
                PingReply = $"Sent: {sent} | Received: {received} | " +
                            $"Loss: {loss:F1}% | Avg RTT: {(received > 0 ? totalRtt / received : 0)} ms";
            });
        }, _continuousPingCts.Token, _writer);
    }

    [RelayCommand]
    private void StopContinuousPing()
    {
        _continuousPingCts?.Cancel();
        _continuousPingCts = null;
        IsContinuousPinging = false;
    }

    private bool CanStartContinuousPing() => !IsContinuousPinging;
    
    
    public PingViewModel(IPingService pingService, IWriter writer) : base("Ping", AppPageNames.Ping)
    {
        _pingService = pingService;
        _writer = writer;
    }

    public PingViewModel()
    {
        _writer = new NullWriter();
        _pingService = new DesignPingService();
    }
}

public class NullWriter : IWriter
{
    public string GetAllLogs()
    {
        throw new NotImplementedException();
    }

    public void Write(string message)
    {
    }

    public void Print()
    {
        throw new NotImplementedException();
    }

    public Task ExportLogsAsync(Visual visual, string fileName)
    {
        throw new NotImplementedException();
    }

    public void ClearLogs()
    {
        throw new NotImplementedException();
    }
}

public class DesignPingService : IPingService
{
    public Task<PingResult> PingOnceAsync(PingRequest request, IWriter writer) =>
        Task.FromResult(new PingResult
        {
            Hostname = "localhost",
            Address = IPAddress.Loopback,
            Success = true,
            RoundtripTime = 12,
            Timestamp = DateTime.Now
        });

    public Task StartContinuousPingAsync(PingRequest request, TimeSpan interval, Action<PingResult> onResult,
        CancellationToken token, IWriter writer) =>
        Task.CompletedTask;
}

