using System.Net;
using vengar.Models;

namespace vengar.Services;

public interface IIpToolsService
{
    IpInfo Calculate(string input, int cidr);
    IEnumerable<string> GenerateRange(string startIp, string endIp);
    bool IsPrivate(string ip);

    // 🔹 NEW
    Task<string?> ResolvePtrAsync(string ip);
}


public class IpToolsService : IIpToolsService
{
    public IpInfo Calculate(string input, int cidr = 24)
    {
        var info = new IpInfo();

        if (IPAddress.TryParse(input, out var ip))
        {
            info.Ip = ip.ToString();
            info.IsPrivate = IsPrivate(ip);

            info.CidrNotation = $"{ip}/{cidr}";

            var network = GetNetworkAddress(ip, cidr);
            var broadcast = GetBroadcastAddress(ip, cidr);

            info.Range = $"{network} - {broadcast}";

            // Network summary
            info.NetworkAddress = network;
            info.BroadcastAddress = broadcast;

            var networkInt = IpToUInt(IPAddress.Parse(network));
            var broadcastInt = IpToUInt(IPAddress.Parse(broadcast));

            info.FirstHost = UIntToIp(networkInt + 1).ToString();
            info.LastHost = UIntToIp(broadcastInt - 1).ToString();
            info.UsableHosts = (int)(broadcastInt - networkInt - 1);

            // Binary / Hex
            var bytes = ip.GetAddressBytes();
            info.Binary = string.Join(".", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            info.Hex = string.Join(".", bytes.Select(b => b.ToString("X2")));
        }

        return info;
    }



    public IEnumerable<string> GenerateRange(string startIp, string endIp)
    {
        if (!IPAddress.TryParse(startIp, out var start)) yield break;
        if (!IPAddress.TryParse(endIp, out var end)) yield break;

        uint startInt = IpToUInt(start);
        uint endInt = IpToUInt(end);

        for (uint i = startInt; i <= endInt; i++)
            yield return UIntToIp(i).ToString();
    }

    public bool IsPrivate(string ip)
    {
        if (!IPAddress.TryParse(ip, out var addr)) return false;
        return IsPrivate(addr);
    }

    private bool IsPrivate(IPAddress ip)
    {
        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            return false; // only IPv4

        var bytes = ip.GetAddressBytes();
        return (bytes[0] == 10) ||
               (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
               (bytes[0] == 192 && bytes[1] == 168);
    }


    private string GetNetworkAddress(IPAddress ip, int cidr)
    {
        uint mask = ~(0xffffffff >> cidr);
        uint ipInt = IpToUInt(ip);
        return UIntToIp(ipInt & mask).ToString();
    }

    private string GetBroadcastAddress(IPAddress ip, int cidr)
    {
        uint mask = ~(0xffffffff >> cidr);
        uint ipInt = IpToUInt(ip);
        return UIntToIp(ipInt | ~mask).ToString();
    }

    private uint IpToUInt(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private IPAddress UIntToIp(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return new IPAddress(bytes);
    }
    
    public async Task<string?> ResolvePtrAsync(string ip)
    {
        if (!IPAddress.TryParse(ip, out var addr))
            return null;

        try
        {
            var entry = await Dns.GetHostEntryAsync(addr);
            return entry.HostName;
        }
        catch
        {
            return null;
        }
    }

}
