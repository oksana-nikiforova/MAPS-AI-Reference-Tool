using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using UglyToad.PdfPig;

namespace MAPSAI.Services.Files
{
    public class FileTextReader
    {
        public string GetFileText(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return "[File not found]";

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            try
            {
                switch (extension)
                {
                    case ".txt":
                        return File.ReadAllText(filePath);

                    case ".csv":
                        return ReadCsv(filePath);

                    case ".json":
                        return ReadJson(filePath);

                    case ".xml":
                        return ReadXml(filePath);

                    case ".pdf":
                        return ReadPdf(filePath);

                    default:
                        return File.ReadAllText(filePath, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                return $"[Error reading {Path.GetFileName(filePath)}: {ex.Message}]";
            }
        }

        private static string ReadCsv(string path)
        {
            var lines = File.ReadAllLines(path);
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                var columns = line.Split(',');
                sb.AppendLine(string.Join(" | ", columns));
            }

            return sb.ToString();
        }

        private static string ReadJson(string path)
        {
            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }

        private static string ReadXml(string path)
        {
            var xdoc = XDocument.Load(path);
            return xdoc.ToString();
        }

        private static string ReadPdf(string path)
        {
            var sb = new StringBuilder();
            using (var pdf = PdfDocument.Open(path))
            {
                foreach (var page in pdf.GetPages())
                    sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }
    }
}
