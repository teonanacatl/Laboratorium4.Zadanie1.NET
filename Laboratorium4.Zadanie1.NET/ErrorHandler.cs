using System.IO;

namespace Laboratorium4.Zadanie1.NET;

public static class ErrorHandler
{
    private static readonly string LogFilePath = "error.log";

    public static void LogError(Exception ex)
    {
        try
        {
            File.AppendAllText(LogFilePath,
                $"{DateTime.Now}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}");
        }
        catch
        {
            // Ignored, as we cannot do much if logging fails
        }
    }
}