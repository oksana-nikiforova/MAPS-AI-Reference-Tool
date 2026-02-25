using MAPSAI.Core.Models;
using System.Collections.ObjectModel;

namespace MAPSAI.Services.Files.Models
{
    public class XmlResponse
    {
        public XmlResponse(bool success, string? error, ObservableCollection<UserStory>? user_stories)
        {
            Success = success;
            Error = error;
            UserStories = user_stories is not null ? user_stories : new();
        }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public ObservableCollection<UserStory> UserStories { get; set; } = [];
    }
}
