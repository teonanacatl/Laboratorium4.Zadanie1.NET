using System.IO;
using System.Security.Cryptography;

namespace Laboratorium4.Zadanie1.NET;

public class FileEncryptor(EncryptionSettings settings) : IFileEncryptor
{
    private readonly EncryptionSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    private SymmetricAlgorithm CreateAlgorithm()
    {
        SymmetricAlgorithm algorithm = _settings.EncryptionMethod switch
        {
            "AES" => Aes.Create(),
            "DES" => DES.Create(),
            "TripleDES" => TripleDES.Create(),
            _ => throw new NotSupportedException($"Encryption method {_settings.EncryptionMethod} is not supported.")
        };

        algorithm.Key = _settings.Key;
        algorithm.IV = _settings.IV;

        return algorithm;
    }

    private void ValidateKeyAndIVLengths()
    {
        switch (_settings.EncryptionMethod)
        {
            case "AES":
                if (_settings.Key.Length != 16 && _settings.Key.Length != 24 && _settings.Key.Length != 32)
                    throw new ArgumentException("Invalid key length for AES. Must be 16, 24, or 32 bytes.");
                if (_settings.IV.Length != 16)
                    throw new ArgumentException("Invalid IV length for AES. Must be 16 bytes.");
                break;
            case "DES":
                if (_settings.Key.Length != 8)
                    throw new ArgumentException("Invalid key length for DES. Must be 8 bytes.");
                if (_settings.IV.Length != 8)
                    throw new ArgumentException("Invalid IV length for DES. Must be 8 bytes.");
                break;
            case "TripleDES":
                if (_settings.Key.Length != 16 && _settings.Key.Length != 24)
                    throw new ArgumentException("Invalid key length for TripleDES. Must be 16 or 24 bytes.");
                if (_settings.IV.Length != 8)
                    throw new ArgumentException("Invalid IV length for TripleDES. Must be 8 bytes.");
                break;
            default:
                throw new NotSupportedException($"Encryption method {_settings.EncryptionMethod} is not supported.");
        }
    }

    public void EncryptFile(string inputFilePath, string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(inputFilePath);
        ArgumentNullException.ThrowIfNull(outputFilePath);

        ValidateKeyAndIVLengths();

        try
        {
            using var algorithm = CreateAlgorithm();
            using var fileStream = new FileStream(outputFilePath, FileMode.Create);
            using var cryptoStream = new CryptoStream(fileStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);
            using var inputStream = new FileStream(inputFilePath, FileMode.Open);

            inputStream.CopyTo(cryptoStream);
            Console.WriteLine($"File encrypted successfully: {inputFilePath} -> {outputFilePath}");
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred during encryption of {inputFilePath}: {ex.Message}");
            throw new Exception("An error occurred during encryption.", ex);
        }
    }

    public void DecryptFile(string inputFilePath, string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(inputFilePath);
        ArgumentNullException.ThrowIfNull(outputFilePath);

        ValidateKeyAndIVLengths();

        try
        {
            using var algorithm = CreateAlgorithm();
            using var fileStream = new FileStream(outputFilePath, FileMode.Create);
            using var cryptoStream = new CryptoStream(fileStream, algorithm.CreateDecryptor(), CryptoStreamMode.Write);
            using var inputStream = new FileStream(inputFilePath, FileMode.Open);

            inputStream.CopyTo(cryptoStream);
            Console.WriteLine($"File decrypted successfully: {inputFilePath} -> {outputFilePath}");
        }
        catch (Exception ex)
        {
            ErrorHandler.LogError(ex);
            Console.WriteLine($"An error occurred during decryption of {inputFilePath}: {ex.Message}");
            throw new Exception("An error occurred during decryption.", ex);
        }
    }

    public static (byte[] Key, byte[] IV) GenerateKeyAndIV(string encryptionMethod)
    {
        switch (encryptionMethod)
        {
            case "AES":
                using (var aes = Aes.Create())
                {
                    aes.GenerateKey();
                    aes.GenerateIV();
                    return (aes.Key, aes.IV);
                }
            case "DES":
                using (var des = DES.Create())
                {
                    des.GenerateKey();
                    des.GenerateIV();
                    return (des.Key, des.IV);
                }
            case "TripleDES":
                using (var tripleDes = TripleDES.Create())
                {
                    tripleDes.GenerateKey();
                    tripleDes.GenerateIV();
                    return (tripleDes.Key, tripleDes.IV);
                }
            default:
                throw new NotSupportedException($"Encryption method {encryptionMethod} is not supported.");
        }
    }
}