using MAPSAI.Models;
using MAPSAI.Services.AI;
using MAPSAI.Views.Backlog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace MAPSAI.Core.Models
{
    public class UserStory : INotifyPropertyChanged
    {

        public UserStory() { }

        public string ID { get; set; } = string.Empty;

        private string _user = string.Empty;
        public string User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged(nameof(User));
                }
            }
        }

        private string _story = string.Empty;
        public string Story
        {
            get => _story;
            set
            {
                if (_story != value)
                {
                    _story = value;
                    OnPropertyChanged(nameof(Story));
                }           
            }
        }

        private string _storyPoints = string.Empty;
        public string StoryPoints
        {
            get => _storyPoints;
            set
            {
                if (_storyPoints != value)
                {
                    _storyPoints = value;
                    OnPropertyChanged(nameof(StoryPoints));
                }
            }
        }

        public GeneratableEntry Purpose { get; set; } = new("So that (Reason or Business value):", "business value of the story", "PurposePrompt", "PurposeMetaPrompt", 192);

        public ListItem<AcceptanceCriterion> AcceptanceCriteria { get; set; } = new("Acceptance criteria", "AcceptanceCriteriaPrompt", "no further instruction", 256);

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private string _priority = string.Empty;
        public string Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        private DateTime startDate = DateTime.Today;
        private DateTime endDate = DateTime.Today;

        public DateTime StartDate
        {
            get => startDate;
            set { startDate = value; OnPropertyChanged(nameof(StartDate)); }
        }

        public DateTime EndDate
        {
            get => endDate;
            set { endDate = value; OnPropertyChanged(nameof(EndDate)); }
        }

        public ObservableCollection<Connection> Connections { get; set; } = [];

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public async Task<string> GeneratePriority(ListEntryService listEntryService)
        {
            try
            {
                var aiRes = await listEntryService.SendPromptRequestAsync("PriorityPrompt");

                if (aiRes == null || string.IsNullOrWhiteSpace(aiRes))
                {
                    return "";
                }

                var json = ExtractJsonObject(aiRes);

                var result = JsonSerializer.Deserialize<PriorityResult>(json);

                return result!.priority;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return "";
            }
        }

        private static string ExtractJsonObject(string input)
        {
            var start = input.IndexOf('{');
            if (start < 0)
                throw new FormatException("No JSON object found.");

            int depth = 0;
            for (int i = start; i < input.Length; i++)
            {
                if (input[i] == '{') depth++;
                else if (input[i] == '}') depth--;

                if (depth == 0)
                    return input.Substring(start, i - start + 1);
            }

            throw new FormatException("Unbalanced JSON object.");
        }
    }
}
