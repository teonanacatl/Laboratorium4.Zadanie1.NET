using System.IO;
using System.Windows;

namespace Laboratorium4.Zadanie1.NET;

public class EncryptionManager(IFileEncryptor fileEncryptor)
{
    private IFileEncryptor _fileEncryptor = fileEncryptor ?? throw new ArgumentNullException(nameof(fileEncryptor));

    private void InitializeFileEncryptor(string keyText, string ivText, string encryptionMethod)
    {
        ArgumentNullException.ThrowIfNull(keyText);
        ArgumentNullException.ThrowIfNull(ivText);
        ArgumentNullException.ThrowIfNull(encryptionMethod);

        try
        {
            var key = Convert.FromBase64String(keyText);
            var iv = Convert.FromBase64String(ivText);

            var settings = new EncryptionSettings
            {
                Key = key,
                IV = iv,
                EncryptionMethod = encryptionMethod
            };

            _fileEncryptor = new FileEncryptor(settings);
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            throw new Exception("An unexpected error occurred while initializing the file encryptor.", ex);
        }
    }

    private bool ValidateKeyAndIV(string keyText, string ivText, string encryptionMethod)
    {
        if (!string.IsNullOrWhiteSpace(keyText) && !string.IsNullOrWhiteSpace(ivText))
            try
            {
                var key = Convert.FromBase64String(keyText);
                var iv = Convert.FromBase64String(ivText);

                switch (encryptionMethod)
                {
                    case "AES":
                        if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                            throw new ArgumentException("Invalid key length for AES. Must be 16, 24, or 32 bytes.");
                        if (iv.Length != 16)
                            throw new ArgumentException("Invalid IV length for AES. Must be 16 bytes.");
                        break;
                    case "DES":
                        if (key.Length != 8)
                            throw new ArgumentException("Invalid key length for DES. Must be 8 bytes.");
                        if (iv.Length != 8)
                            throw new ArgumentException("Invalid IV length for DES. Must be 8 bytes.");
                        break;
                    case "TripleDES":
                        if (key.Length != 16 && key.Length != 24)
                            throw new ArgumentException("Invalid key length for TripleDES. Must be 16 or 24 bytes.");
                        if (iv.Length != 8)
                            throw new ArgumentException("Invalid IV length for TripleDES. Must be 8 bytes.");
                        break;
                    default:
                        throw new NotSupportedException($"Encryption method {encryptionMethod} is not supported.");
                }

                return true;
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid Base64 encoded key and IV.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

        MessageBox.Show("Please enter a valid key and IV.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
    }

    public async Task<IEnumerable<string>> EncryptFilesAsync(IEnumerable<string> inputFilePaths, string keyText,
        string ivText, string encryptionMethod)
    {
        if (!ValidateKeyAndIV(keyText, ivText, encryptionMethod)) return Array.Empty<string>();

        InitializeFileEncryptor(keyText, ivText, encryptionMethod);

        var tempDirectory = AppConfig.GetTempDirectory();
        Directory.CreateDirectory(tempDirectory);

        var encryptedFiles = new List<string>();
        var tasks = inputFilePaths.Select(async inputFilePath =>
        {
            try
            {
                var uniqueFilePath = FileHelper.GetUniqueFilePath(tempDirectory, Path.GetFileName(inputFilePath));
                _fileEncryptor.EncryptFile(inputFilePath, uniqueFilePath);

                var encryptedFilePath = uniqueFilePath + ".enc";
                File.Move(uniqueFilePath, encryptedFilePath, true);

                encryptedFiles.Add(encryptedFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encrypting file {inputFilePath}: {ex.Message}");
            }
        });

        await Task.WhenAll(tasks);
        MessageBox.Show("Files encrypted successfully.");
        return encryptedFiles;
    }

    public async Task<IEnumerable<string>> DecryptFilesAsync(IEnumerable<string> inputFilePaths, string keyText,
        string ivText, string encryptionMethod)
    {
        if (!ValidateKeyAndIV(keyText, ivText, encryptionMethod)) return Array.Empty<string>();

        InitializeFileEncryptor(keyText, ivText, encryptionMethod);

        var tempDirectory = AppConfig.GetTempDirectory();
        Directory.CreateDirectory(tempDirectory);

        var decryptedFiles = new List<string>();
        var tasks = inputFilePaths.Select(async inputFilePath =>
        {
            try
            {
                var originalFileName = FileHelper.GetOriginalFileName(inputFilePath);
                var uniqueFilePath = FileHelper.GetUniqueFilePath(tempDirectory, originalFileName);
                _fileEncryptor.DecryptFile(inputFilePath, uniqueFilePath);
                var decryptedFilePath = Path.Combine(tempDirectory, originalFileName);
                File.Move(uniqueFilePath, decryptedFilePath, true);
                decryptedFiles.Add(decryptedFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting file {inputFilePath}: {ex.Message}");
            }
        });

        await Task.WhenAll(tasks);
        MessageBox.Show("Files decrypted successfully.");
        return decryptedFiles;
    }

    public void UpdateKeyAndIVForAlgorithm(string encryptionMethod, out string keyText, out string ivText)
    {
        var (key, iv) = FileEncryptor.GenerateKeyAndIV(encryptionMethod);
        keyText = Convert.ToBase64String(key);
        ivText = Convert.ToBase64String(iv);
    }

    public static void ClearTempDirectory()
    {
        try
        {
            var directoryInfo = new DirectoryInfo(AppConfig.GetTempDirectory());
            foreach (var file in directoryInfo.GetFiles()) file.Delete();
            foreach (var dir in directoryInfo.GetDirectories()) dir.Delete(true);
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred while clearing the temp directory: {ex.Message}");
        }
    }
}