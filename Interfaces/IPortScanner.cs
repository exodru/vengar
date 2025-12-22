using vengar.Models;

namespace vengar.Interfaces;

public interface IPortScanner
{
    Task<IReadOnlyList<PortScanEntry>> ScanAsync(
        IWriter writer,
        string host,
        IEnumerable<int> ports,
        int timeoutMs = 1500,
        CancellationToken token = default);
}
