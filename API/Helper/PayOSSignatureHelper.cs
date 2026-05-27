using System.Security.Cryptography;
using System.Text;

namespace API.Helper
{
    public static class PayOSSignatureHelper
    {
        public static string ComputeSignature(string rawBody, string checksumKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
            return Convert.ToHexString(hash).ToLower();
        }

        public static bool Verify(string rawBody, string signature, string checksumKey)
        {
            var computed = ComputeSignature(rawBody, checksumKey);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes(signature ?? "")
            );
        }
    }
}
