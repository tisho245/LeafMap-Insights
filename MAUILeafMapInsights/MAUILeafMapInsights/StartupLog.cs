using System.Diagnostics;
using System.Text;

namespace MAUILeafMapInsights;

/// <summary>Writes startup steps and exceptions to a file so we can see what happens when the app crashes with no UI.</summary>
public static class StartupLog
{
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LeafMapInsights_startup.log");

    private static void Write(string message)
    {
        try
        {
            var line = $"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}{Environment.NewLine}";
            System.IO.File.AppendAllText(Path, line, Encoding.UTF8);
            Debug.WriteLine(message);
        }
        catch { /* ignore */ }
    }

    public static void Info(string message) => Write("OK: " + message);
    public static void Error(string message) => Write("ERR: " + message);
    public static void Error(Exception ex) => Write("ERR: " + ex.ToString());

    public static string LogPath => Path;
}
