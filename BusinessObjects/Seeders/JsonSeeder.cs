using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessObjects.Seeders
{
    public class JsonSeeder
    {
        public static async Task<List<T>> SeedingFromJson<T>(string fileName)
        {
            var path = Path.GetFullPath(Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "..",
                    "BusinessObjects",
                    "Seeders",
                    "Data",
                    $"{fileName}.json"
                ));

            if (!File.Exists(path))
            {
                Console.WriteLine($"users.json not found: {path}");
                return null;
            }

            var json = await File.ReadAllTextAsync(path);

            var objs = JsonSerializer.Deserialize<List<T>>(json);

            if (objs == null || objs.Count == 0)
                return null;

            return objs;
        }
    }
}
