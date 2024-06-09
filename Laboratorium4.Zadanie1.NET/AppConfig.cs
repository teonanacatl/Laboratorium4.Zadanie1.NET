using System.IO;
using System.Net;

namespace Laboratorium4.Zadanie1.NET;

public static class AppConfig
{
    public const int Port = 5000;
    public static readonly IPAddress ServerIp = IPAddress.Parse("127.0.0.1");

    public static string EncryptionSettingsFilePath => Path.Combine(GetAppRootDirectory(), "encryptionSettings.json");

    public static string GetAppRootDirectory()
    {
        var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(exePath);
    }

    public static string GetFileStoragePath()
    {
        var rootDirectory = GetAppRootDirectory();
        var fileStoragePath = Path.Combine(rootDirectory, "FileStorage");

        if (!Directory.Exists(fileStoragePath)) Directory.CreateDirectory(fileStoragePath);

        return fileStoragePath;
    }

    public static string GetTempDirectory()
    {
        var rootDirectory = GetAppRootDirectory();
        var tempDirectory = Path.Combine(rootDirectory, "TempFiles");

        if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

        return tempDirectory;
    }
}