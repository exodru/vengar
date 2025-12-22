using DnsClient;
using DnsClient.Protocol;
using vengar.Interfaces;
using vengar.Models;

namespace vengar.Services;

public class DnsLookupService : IDnsLookup
{
    private readonly LookupClient _client;

    public DnsLookupService()
    {
        var options = new LookupClientOptions { UseCache = true, Timeout = TimeSpan.FromSeconds(5), Retries = 2, };
        _client = new LookupClient(options);
    }

    public async Task<DnsResult> LookupAsync(IWriter writer, string host, CancellationToken token = default)
    {
        var result = new DnsResult { Hostname = host };
        if (string.IsNullOrWhiteSpace(host))
        {
            writer.Write("[DNS][ERROR] Hostname is empty");
            result.Error = "Hostname is empty";
            return result;
        }

        try
        {
            writer.Write($"[DNS] Starting full lookup for: {host}");
            var types = new[]
            {
                QueryType.A, QueryType.AAAA, QueryType.CNAME, QueryType.MX, QueryType.NS, QueryType.TXT,
                QueryType.SOA, QueryType.CAA
            };
            foreach (var type in types)
            {
                writer.Write($"[DNS] Querying {type} records...");
                var response = await _client.QueryAsync(host, type, cancellationToken: token);
                if (response.HasError)
                {
                    writer.Write($"[DNS][ERROR] {type} query error: {response.ErrorMessage}");
                    continue;
                }

                if (!response.Answers.Any())
                {
                    writer.Write($"[DNS] No {type} records found");
                    continue;
                }

                foreach (var record in response.Answers)
                {
                    var parsed = ParseRecord(record);
                    result.Records.Add(parsed);
                    writer.Write(
                        $"[DNS] {parsed.Type} record: Name={parsed.Name}, Value={parsed.Value}, TTL={parsed.Ttl}");
                }
            }

            result.Success = result.Records.Count > 0;
            writer.Write($"[DNS] Lookup completed for {host}, {result.Records.Count} records found");
        }
        catch (Exception ex)
        {
            writer.Write($"[DNS][ERROR] Exception occurred: {ex.Message}");
            result.Error = ex.Message;
        }

        return result;
    }

    private DnsRecordEntry ParseRecord(DnsResourceRecord record)
    {
        switch (record)
        {
            case ARecord a:
                return new DnsRecordEntry
                {
                    Type = "A", Name = a.DomainName.Value, Value = a.Address.ToString(), Ttl = a.InitialTimeToLive
                };
            case AaaaRecord aaaa:
                return new DnsRecordEntry
                {
                    Type = "AAAA",
                    Name = aaaa.DomainName.Value,
                    Value = aaaa.Address.ToString(),
                    Ttl = aaaa.InitialTimeToLive
                };
            case CNameRecord cname:
                return new DnsRecordEntry
                {
                    Type = "CNAME",
                    Name = cname.DomainName.Value,
                    Value = cname.CanonicalName.Value,
                    Ttl = cname.InitialTimeToLive
                };
            case MxRecord mx:
                return new DnsRecordEntry
                {
                    Type = "MX",
                    Name = mx.DomainName.Value,
                    Value = $"{mx.Preference} {mx.Exchange.Value}",
                    Ttl = mx.InitialTimeToLive
                };
            case NsRecord ns:
                return new DnsRecordEntry
                {
                    Type = "NS", Name = ns.DomainName.Value, Value = ns.NSDName.Value, Ttl = ns.InitialTimeToLive
                };
            case TxtRecord txt:
                return new DnsRecordEntry
                {
                    Type = "TXT",
                    Name = txt.DomainName.Value,
                    Value = string.Join(" ", txt.Text),
                    Ttl = txt.InitialTimeToLive
                };
            case SoaRecord soa:
                return new DnsRecordEntry
                {
                    Type = "SOA",
                    Name = soa.DomainName.Value,
                    Value = $"{soa.MName.Value} {soa.RName.Value}",
                    Ttl = soa.InitialTimeToLive
                };
            case CaaRecord caa:
                return new DnsRecordEntry
                {
                    Type = "CAA",
                    Name = caa.DomainName.Value,
                    Value = $"{caa.Tag} {caa.Value}",
                    Ttl = caa.InitialTimeToLive
                };
            default:
                return new DnsRecordEntry
                {
                    Type = record.RecordType.ToString(),
                    Name = record.DomainName.Value,
                    Value = record.ToString(),
                    Ttl = record.InitialTimeToLive
                };
        }
    }

}