namespace vengar.Interfaces;

public interface IPing
{
    public string Address { get; set; }
    public string Hostname { get; set; }
    public long Rtt { get; set; }
    public int BufferSize { get; set; }
    public int Success {get; set;}
    public int Failure {get; set;}
}