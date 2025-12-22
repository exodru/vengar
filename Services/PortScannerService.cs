using System.Net.Sockets;
using vengar.Interfaces;
using vengar.Models;

namespace vengar.Services;

public class PortScannerService : IPortScanner
{
    public async Task<IReadOnlyList<PortScanEntry>> ScanAsync(IWriter writer, string host, IEnumerable<int> ports,
        int timeoutMs = 1500, CancellationToken token = default)
    {
        var results = new List<PortScanEntry>();
        writer.Write($"[PORTSCAN] Starting scan → {host}");
        foreach (var port in ports.Distinct().OrderBy(p => p))
        {
            var entry = new PortScanEntry
            {
                Port = port, Title = KnownPorts.TryGetValue(port, out var name) ? name : "Unknown"
            };
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var completed = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, token));
                if (completed == connectTask && client.Connected)
                {
                    entry.Status = PortScanStatus.Open;
                    writer.Write($"[PORTSCAN] {port}/TCP OPEN");
                }
                else
                {
                    entry.Status = PortScanStatus.TimedOut;
                    writer.Write($"[PORTSCAN] {port}/TCP timed out");
                }
            }
            catch (SocketException ex)
            {
                entry.Status = PortScanStatus.Closed;
                writer.Write($"[PORTSCAN] {port}/TCP closed ({ex.SocketErrorCode})");
            }
            catch (Exception ex)
            {
                entry.Status = PortScanStatus.Error;
                writer.Write($"[PORTSCAN][ERROR] {port}/TCP → {ex.Message}");
            }

            results.Add(entry);
        }

        writer.Write($"[PORTSCAN] Scan finished → {results.Count} ports");
        return results;
    }

    private static readonly Dictionary<int, string> KnownPorts = new()
    {
        // File transfer / remote access
        { 20, "FTP-Data" },
        { 21, "FTP" },
        { 22, "SSH" },
        { 23, "Telnet" },
        { 3389, "RDP" },
        { 5900, "VNC" },
        { 1723, "PPTP VPN" },
        { 1701, "L2TP VPN" },

        // Mail
        { 25, "SMTP" },
        { 587, "SMTP Submission" },
        { 2525, "SMTP Alternate" },
        { 110, "POP3" },
        { 995, "POP3S" },
        { 143, "IMAP" },
        { 993, "IMAPS" },

        // Web
        { 80, "HTTP" },
        { 443, "HTTPS" },
        { 8080, "HTTP Proxy" },
        { 8008, "HTTP Alt" },
        { 8443, "HTTPS Alt" },
        { 4443, "HTTPS Alt" },

        // Name / directory services
        { 53, "DNS" },
        { 111, "RPCBind" },
        { 135, "MS RPC" },
        { 137, "NetBIOS Name" },
        { 138, "NetBIOS Datagram" },
        { 139, "NetBIOS Session" },
        { 445, "SMB / Microsoft-DS" },
        { 548, "AFP (Apple File Sharing)" },

        // Databases
        { 1433, "Microsoft SQL Server" },
        { 3306, "MySQL" },
        { 5432, "PostgreSQL" },

        // Messaging / misc
        { 1863, "MSN Messenger" },
        { 5190, "AIM / ICQ" },
        { 6891, "BitTorrent" },
        { 6667, "IRC" },
        { 1503, "NetMeeting" },
        { 5050, "Multimedia / Streaming" },
        { 515, "LPD Printer" },
        { 631, "IPP Printing" },
        { 502, "Modbus" },
        { 3282, "Apple Remote Desktop" },
        { 5631, "PCAnywhere" },
        { 5632, "PCAnywhere Data" }
    };
}