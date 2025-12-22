namespace vengar.Models;

public class IpInfo
{
    public string Ip { get; set; } = "";
    public bool IsPrivate { get; set; }

    public string CidrNotation { get; set; } = "";
    public string Range { get; set; } = "";

    public string Binary { get; set; } = "";
    public string Hex { get; set; } = "";

    public string NetworkAddress { get; set; } = "";
    public string BroadcastAddress { get; set; } = "";
    public string FirstHost { get; set; } = "";
    public string LastHost { get; set; } = "";
    public int UsableHosts { get; set; }

    public string PtrRecord { get; set; } = "";
}

