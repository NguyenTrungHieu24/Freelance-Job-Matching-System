using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Helper
{
    public static class PayOSSignatureHelper
    {
        public static bool Verify(object data, string signature, string checksumKey)
        {
            if (string.IsNullOrEmpty(signature)) return false;

            var dict = ToSortedDictionary(data);
            var parts = new List<string>();

            foreach (var kvp in dict)
            {
                string valueStr = NormalizeValue(kvp.Value);
                parts.Add($"{kvp.Key}={valueStr}");
            }

            string raw = string.Join("&", parts);

            Console.WriteLine("Raw string for signature: " + raw);   // Quan trong de debug

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(raw));
            string computed = Convert.ToHexString(hash).ToLowerInvariant();

            Console.WriteLine("Computed signature: " + computed);
            Console.WriteLine("Received signature : " + signature);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes(signature)
            );
        }

        private static SortedDictionary<string, object> ToSortedDictionary(object obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            return new SortedDictionary<string, object>(dict ?? new());
        }

        private static string NormalizeValue(object? value)
        {
            if (value == null) return "";

            if (value is JsonElement je)
            {
                return je.ValueKind switch
                {
                    JsonValueKind.String => je.GetString() ?? "",
                    JsonValueKind.Number => je.GetRawText(),           // Giu nguyen so khong co dau "
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "",
                    JsonValueKind.Object => SerializeSortedObject(je),
                    JsonValueKind.Array => SerializeArray(je),
                    _ => je.GetRawText()
                };
            }

            return value.ToString() ?? "";
        }

        private static string SerializeSortedObject(JsonElement obj)
        {
            var dict = new SortedDictionary<string, string>();
            foreach (var prop in obj.EnumerateObject())
            {
                dict[prop.Name] = NormalizeValue(prop.Value);   // De quy
            }

            return JsonSerializer.Serialize(dict, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private static string SerializeArray(JsonElement arr)
        {
            var list = new List<string>();
            foreach (var item in arr.EnumerateArray())
            {
                list.Add(NormalizeValue(item));
            }
            return JsonSerializer.Serialize(list);
        }
    }
}