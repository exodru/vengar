using Avalonia;

namespace vengar.Interfaces;

public interface IWriter
{
    public string GetAllLogs();
    public void Write(string text);
    public void Print();
    public Task ExportLogsAsync(Visual visual, string fileName);
    public void ClearLogs();
}