using MigraDoc.DocumentObjectModel;

namespace MAPSAI.Services.Files.Models
{
    public class DocumentResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public Section? Section { get; set; } = new();
        public Document? Document { get; set; } = new();
    }
}
