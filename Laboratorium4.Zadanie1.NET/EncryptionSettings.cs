namespace Laboratorium4.Zadanie1.NET;

public class EncryptionSettings
{
    public string EncryptionMethod { get; init; }
    public byte[] Key { get; init; }
    public byte[] IV { get; init; }
}