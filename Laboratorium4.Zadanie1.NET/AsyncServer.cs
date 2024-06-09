using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Laboratorium4.Zadanie1.NET;

public static class AsyncServer
{
    private static readonly string FileDirectory = AppConfig.GetFileStoragePath();
    private static readonly SemaphoreSlim FileAccessSemaphore = new(1, 1);

    private static TcpListener? _listener;
    private static CancellationTokenSource? _cancellationTokenSource;

    private const string ErrorString = "ERROR";

    public static async Task StartServerAsync()
    {
        _listener = new TcpListener(AppConfig.ServerIp, AppConfig.Port);
        _listener.Start();
        _cancellationTokenSource = new CancellationTokenSource();
        Console.WriteLine("Server started, waiting for connections...");

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected.");
            _ = HandleClientAsync(client, _cancellationTokenSource.Token);
        }
    }

    public static void StopServer()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = client.GetStream();
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received: {receivedData}");

            if (receivedData.StartsWith("SEND_FILE"))
                await HandleSendFileRequest(stream, receivedData, cancellationToken);
            else if (receivedData.StartsWith("LIST_FILES"))
                await HandleListFilesRequest(stream, cancellationToken);
            else if (receivedData.StartsWith("GET_FILE"))
                await HandleGetFileRequest(stream, receivedData, cancellationToken);
            else
                await SendResponse(stream, "INVALID_COMMAND", cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private static async Task HandleSendFileRequest(NetworkStream stream, string receivedData,
        CancellationToken cancellationToken)
    {
        var fileName = receivedData.Split(' ')[1];
        var filePath = Path.Combine(FileDirectory, fileName);

        try
        {
            await FileAccessSemaphore.WaitAsync(cancellationToken);

            Console.WriteLine($"Receiving file: {filePath}");

            await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[4096];
                long totalBytesReceived = 0;

                var lengthBuffer = new byte[sizeof(int)];
                var bytesRead = await stream.ReadAsync(lengthBuffer.AsMemory(0, sizeof(int)), cancellationToken);
                if (bytesRead < sizeof(int))
                {
                    Console.WriteLine("Failed to read file length.");
                    await SendResponse(stream, ErrorString, cancellationToken);
                    return;
                }

                var fileLength = BitConverter.ToInt32(lengthBuffer, 0);
                Console.WriteLine($"Expected file length: {fileLength} bytes");

                while (totalBytesReceived < fileLength &&
                       (bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalBytesReceived += bytesRead;
                    Console.WriteLine($"Received {totalBytesReceived} / {fileLength} bytes");
                }

                if (totalBytesReceived != fileLength)
                {
                    Console.WriteLine(
                        $"File transfer incomplete. Received {totalBytesReceived} out of {fileLength} bytes.");
                    await SendResponse(stream, ErrorString, cancellationToken);
                    return;
                }

                Console.WriteLine($"File {fileName} received and stored. Size: {totalBytesReceived} bytes.");
            }

            await SendResponse(stream, "FILE_RECEIVED", cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred while handling send file request: {ex.Message}");
            await SendResponse(stream, ErrorString, cancellationToken);
        }
        finally
        {
            FileAccessSemaphore.Release();
        }
    }


    private static async Task HandleListFilesRequest(NetworkStream stream, CancellationToken cancellationToken)
    {
        try
        {
            var files = Directory.GetFiles(FileDirectory);
            var fileList = string.Join(",", files.Select(Path.GetFileName));
            Console.WriteLine("Sending file list: " + fileList);
            await SendResponse(stream, fileList, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred while handling list files request: {ex.Message}");
            await SendResponse(stream, ErrorString, cancellationToken);
        }
    }

    private static async Task HandleGetFileRequest(NetworkStream stream, string receivedData,
        CancellationToken cancellationToken)
    {
        var fileName = receivedData.Split(' ')[1];
        var filePath = Path.Combine(FileDirectory, fileName);

        if (File.Exists(filePath))
            try
            {
                await FileAccessSemaphore.WaitAsync(cancellationToken);

                byte[] fileData;

                await using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileData = new byte[fileStream.Length];
                    await fileStream.ReadAsync(fileData, cancellationToken);
                    Console.WriteLine($"Read file data: {fileData.Length} bytes");
                }

                await stream.WriteAsync(fileData, cancellationToken);
                Console.WriteLine($"File {fileName} sent to client. Size: {fileData.Length} bytes.");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex);
                Console.WriteLine($"An error occurred while sending file {fileName}: {ex.Message}");
                await SendResponse(stream, ErrorString, cancellationToken);
            }
            finally
            {
                FileAccessSemaphore.Release();
            }
        else
            await SendResponse(stream, "FILE_NOT_FOUND", cancellationToken);
    }

    private static async Task SendResponse(NetworkStream stream, string response, CancellationToken cancellationToken)
    {
        var responseData = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(responseData, cancellationToken);
    }
}