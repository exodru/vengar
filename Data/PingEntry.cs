using System.Net;
using vengar.Interfaces;

namespace vengar.Data;

public class PingEntry : IPing
{
    // Primary Constructor (Still required if you want to use it elsewhere)
    public PingEntry(string host, string address, int port, int rtt, int buffersize, int success, int failure)
    {
        Hostname = host;
        Address = address;
        Rtt = rtt;
        BufferSize = buffersize;
        Success = success;
        Failure = failure;
    }
    
    // **NEW: Parameterless Constructor**
    public PingEntry()
    {
        Hostname = "";
        Address = "";
        Rtt = 0;
        BufferSize = 0;
        Success = 0;
        Failure = 0;
    }

    public string Hostname { get; set; }
    public string? Address { get; set; }
    public long Rtt { get; set; }
    public int BufferSize { get; set; }
    public int Success { get; set; }
    public int Failure { get; set; }
}