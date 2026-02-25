using MAPSAI.Core.Models;
using System.Collections.ObjectModel;

namespace MAPSAI.Models
{
    public class ExcelResponse
    {
        public ExcelResponse(bool success, string? error, Dictionary<string, string>? project_Info, ObservableCollection<UserStory>? user_stories)
        {
            Success = success;
            Error = error;
            ProjectInfo = project_Info is not null ? project_Info : new();
            UserStories = user_stories is not null ? user_stories : new();
        }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public Dictionary<string, string> ProjectInfo { get; set; } = [];
        public ObservableCollection<UserStory> UserStories { get; set; } = [];
    }
}
