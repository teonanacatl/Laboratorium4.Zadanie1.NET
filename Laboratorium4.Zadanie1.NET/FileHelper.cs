using System.IO;

namespace Laboratorium4.Zadanie1.NET;

public static class FileHelper
{
    public static string GetUniqueFilePath(string directory, string fileName)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        return Path.Combine(directory, uniqueFileName);
    }

    public static string GetOriginalFileName(string uniqueFilePath)
    {
        var fileName = Path.GetFileName(uniqueFilePath);
        var originalFileName = fileName[(fileName.IndexOf('_') + 1)..];
        return originalFileName;
    }
}