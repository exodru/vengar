using vengar.Models;

namespace vengar.Interfaces
{
    public class DnsResult
    {
        public string Hostname { get; set; } = "";
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<DnsRecordEntry> Records { get; set; } = [];
    }


    public interface IDnsLookup
    {
        Task<DnsResult> LookupAsync(IWriter writer, string host, CancellationToken token = default);
    }
}
