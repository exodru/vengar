
namespace vengar.Helpers;

public static class DnsRecordHelp
{
    public static readonly Dictionary<string, string> Descriptions = new()
    {
        ["A"] = "Maps a hostname to an IPv4 address.",
        ["AAAA"] = "Maps a hostname to an IPv6 address.",
        ["CNAME"] = "Creates an alias for another domain.",
        ["MX"] = "Specifies mail servers for a domain.",
        ["NS"] = "Specifies authoritative name servers.",
        ["PTR"] = "Reverse DNS: IP → hostname.",
        ["SRV"] = "Specifies service location.",
        ["SOA"] = "Administrative info about the domain.",
        ["TXT"] = "Stores arbitrary text (SPF, DKIM, DMARC).",
        ["CAA"] = "Specifies which CAs can issue certificates.",
        ["DS"] = "DNSSEC delegation signer record.",
        ["DNSKEY"] = "DNSSEC public signing key."
    };

    public static string Get(string type) =>
        Descriptions.TryGetValue(type, out var desc)
            ? desc
            : "Unknown DNS record type.";
}


