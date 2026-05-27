namespace API.Helper;

public class FileValidateHelper
{
    private static readonly byte[] jpgMagic = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] pngMagic = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] pdfMagic = { 0x25, 0x50, 0x44, 0x46 };
    private static readonly byte[] docxMagic = { 0x50, 0x4B, 0x03, 0x04 };
    private static readonly byte[] docMagic = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
    
    // validate file for avatar
    public static bool IsAvatarValid(IFormFile file)
    {
        var allowedExtension = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtension.Contains(extension))
            return false;

        using var stream = file.OpenReadStream();
        var header = new byte[8];
        var bytesRead = stream.Read(header, 0, 8);
        if (bytesRead < 3)
        {
            return false;
        }

        var signature = new Dictionary<string, byte[]>
        {
            [".jpg"] = jpgMagic,
            [".jpeg"] = jpgMagic,
            [".png"] = pngMagic,
        };
        bool check = signature.TryGetValue(extension, out var value) &&
            header.Take(value.Length).SequenceEqual(value);

        if (!check) return false;
        return true;
    }

    public static bool IsCvFileValid(IFormFile file)
    {
        var allowedExtension = new[] { ".pdf", ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtension.Contains(extension))
            return false;

        using var stream = file.OpenReadStream();
        var header = new byte[8];
        var bytesRead = stream.Read(header, 0, 8);
        if (bytesRead < 4)
        {
            return false;
        }

        var signature = new Dictionary<string, byte[]>
        {
            [".pdf"] = pdfMagic,
            [".doc"] = docMagic,
            [".docx"] = docxMagic,
        };
        bool check = signature.TryGetValue(extension, out var value) &&
                     header.Take(value.Length).SequenceEqual(value);

        if (!check) return false;
        return true;
    }
}