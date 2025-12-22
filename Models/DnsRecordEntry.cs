namespace vengar.Models;

public class DnsRecordEntry
{
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public int Ttl { get; set; }
}
