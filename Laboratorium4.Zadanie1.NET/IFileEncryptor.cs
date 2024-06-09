namespace Laboratorium4.Zadanie1.NET;

public interface IFileEncryptor
{
    void EncryptFile(string inputFilePath, string outputFilePath);
    void DecryptFile(string inputFilePath, string outputFilePath);
}