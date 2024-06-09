using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Laboratorium4.Zadanie1.NET;

public class AsyncClient
{
    private static bool IsFileLocked(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            stream.Close();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"File {filePath} is locked. Exception: {ex.Message}");
            return true;
        }

        return false;
    }

    public async Task<bool> SendFileAsync(string filePath, string originalFileName)
    {
        try
        {
            if (IsFileLocked(filePath))
            {
                Console.WriteLine($"File {filePath} is currently locked by another process.");
                return false;
            }

            var client = new TcpClient();
            await client.ConnectAsync(AppConfig.ServerIp, AppConfig.Port);
            Console.WriteLine("Connected to server.");

            await using var stream = client.GetStream();
            var command = $"SEND_FILE {originalFileName}";
            var commandData = Encoding.UTF8.GetBytes(command);
            await stream.WriteAsync(commandData, 0, commandData.Length);
            Console.WriteLine("File send command sent to server.");

            var fileData = await File.ReadAllBytesAsync(filePath);
            Console.WriteLine($"Sending file: {filePath}, Size: {fileData.Length} bytes.");

            var fileLengthData = BitConverter.GetBytes(fileData.Length);
            await stream.WriteAsync(fileLengthData, 0, fileLengthData.Length);

            await stream.WriteAsync(fileData, 0, fileData.Length);
            Console.WriteLine("File sent to server.");

            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Server response: {response}");

            client.Close();
            Console.WriteLine("Client disconnected.");
            return true;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }

    public async Task<string[]> RetrieveFileListAsync()
    {
        try
        {
            var client = new TcpClient();
            await client.ConnectAsync(AppConfig.ServerIp, AppConfig.Port);
            Console.WriteLine("Connected to server.");

            await using var stream = client.GetStream();
            var request = "LIST_FILES"u8.ToArray();
            await stream.WriteAsync(request, 0, request.Length);
            Console.WriteLine("File list request sent to server.");

            var buffer = new byte[4096];
            var bytesRead = await stream.ReadAsync(buffer);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var files = response.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine($"Files retrieved from server: {string.Join(", ", files)}");
            return files;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred while retrieving file list: {ex.Message}");
            return [];
        }
    }

    public async Task<string> RetrieveFileAsync(string fileName)
    {
        try
        {
            var client = new TcpClient();
            await client.ConnectAsync(AppConfig.ServerIp, AppConfig.Port);
            Console.WriteLine("Connected to server.");

            await using var stream = client.GetStream();
            var request = Encoding.UTF8.GetBytes($"GET_FILE {fileName}");
            await stream.WriteAsync(request, 0, request.Length);
            Console.WriteLine("File retrieval request sent to server.");

            var tempDirectory = AppConfig.GetTempDirectory();
            Directory.CreateDirectory(tempDirectory);

            var tempFilePath = Path.Combine(tempDirectory, fileName);

            await using var outputFileStream =
                new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[4096];
            int bytesRead;
            long totalBytesReceived = 0;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                await outputFileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesReceived += bytesRead;
            }

            Console.WriteLine($"File {fileName} received. Size: {totalBytesReceived} bytes.");

            return tempFilePath;
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }
}