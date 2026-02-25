namespace MAPSAI.Models.AI
{

    public class AiCallResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Response { get; set; }
        public double? TotalDuration { get; set; }
    }

}
