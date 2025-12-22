using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using vengar.Interfaces;

namespace vengar.Services;

public class FileWriter : IWriter
{
    private readonly List<string> _actionLog = new();
    private readonly object _fileLock = new();
    public string GetAllLogs() => string.Join(Environment.NewLine, _actionLog);

    public void Write(string log)
    {
        var timestampEntry = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        lock (_fileLock)
        {
            Console.WriteLine($"LOGGER: {timestampEntry}: {log}");
            _actionLog.Add($"[{timestampEntry}]: {log}");
        }
    }

    public void Print()
    {
        foreach (var log in _actionLog)
        {
            Console.WriteLine(log);
        }
    }

    public async Task ExportLogsAsync(Visual visual, string fileName)
    {
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save Logs",
                SuggestedFileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmm}",
                FileTypeChoices =
                [
                    new FilePickerFileType("Text File") { Patterns = ["*.txt"] },
                    new FilePickerFileType("CSV File") { Patterns = ["*.csv"] }
                ]
            });

        if (file is null) return;

        await using var stream = await file.OpenWriteAsync();
        using var writer = new StreamWriter(stream);

        if (file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            await WriteCsvAsync(writer);
        }
        else
        {
            await writer.WriteAsync(GetAllLogs());
        }
    }

    private Task WriteCsvAsync(StreamWriter writer)
    {
        lock (_fileLock)
        {
            writer.WriteLine("Timestamp,Message");

            foreach (var log in _actionLog)
            {
                // "[2025-01-01 12:00:00.000]: message"
                var idx = log.IndexOf("]:", StringComparison.Ordinal);
                if (idx > 0)
                {
                    var ts = log[1..idx];
                    var msg = log[(idx + 3)..].Replace("\"", "\"\"");
                    writer.WriteLine($"\"{ts}\",\"{msg}\"");
                }
            }
        }

        return Task.CompletedTask;
    }

    public void ClearLogs()
    {
        lock (_fileLock)
        {
           _actionLog.Clear(); 
        }
    }
}