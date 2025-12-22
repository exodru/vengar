namespace vengar.Models;

public class PortScanEntry
{
    public string Title { get; set; } = "";
    public int Port { get; set; }
    public PortScanStatus Status { get; set; }
}

public enum PortScanStatus
{
    Open,
    Closed,
    TimedOut,
    Error
}
