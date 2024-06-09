namespace Laboratorium4.Zadanie1.NET;

public class FileTransferManager(AsyncClient asyncClient)
{
    private readonly AsyncClient _asyncClient = asyncClient ?? throw new ArgumentNullException(nameof(asyncClient));

    public async Task<bool> UploadFileAsync(string filePath, string originalFileName)
    {
        return await _asyncClient.SendFileAsync(filePath, originalFileName);
    }

    public async Task<string> RetrieveFileAsync(string fileName)
    {
        return await _asyncClient.RetrieveFileAsync(fileName);
    }

    public async Task<IEnumerable<string>> GetAvailableFilesAsync()
    {
        return await _asyncClient.RetrieveFileListAsync();
    }
}