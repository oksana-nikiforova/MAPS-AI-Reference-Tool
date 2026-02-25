using MAPSAI.Models;
using MAPSAI.Models.AI;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MAPSAI.Core.Models
{
    public sealed class SettingsModel
    {
        private SettingsModel() { }

        private static readonly Lazy<SettingsModel> lazy = new Lazy<SettingsModel>(() => new SettingsModel());
        public static SettingsModel Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private string _jira_project_key = string.Empty;
        private string _jira_email = string.Empty;
        private string _jira_link = string.Empty;
        private string _jira_api_key = string.Empty;
        private string _leantime_url = string.Empty;
        private string _leantime_api_key = string.Empty;
        private string _chatgpt_api_key = string.Empty;
        private bool _storyPoints = false;

        public bool StoryPoints
        {
            get => _storyPoints;
            set
            {
                _storyPoints = value;
                SaveToSecureStorage("Story_Points", value.ToString());
                OnPropertyChanged(nameof(StoryPoints));
            }
        }

        public string JiraProjectKey 
        { 
            get => _jira_project_key;
            set 
            {
                _jira_project_key = value;
                SaveToSecureStorage("Jira_Project_Key", value);
            }
        }
        public string JiraEmail 
        { 
            get => _jira_email;
            set 
            {
                _jira_email = value;
                SaveToSecureStorage("Jira_Email", value);
            }
        }
        public string JiraLink 
        { 
            get => _jira_link;
            set
            {
                _jira_link = value;
                SaveToSecureStorage("Jira_Link", value);
            }
        }
        public string JiraApiKey
        { 
            get => _jira_api_key;
            set
            {
                _jira_api_key = value;
                SaveToSecureStorage("Jira_Api_Key", value);
            } 
        }
        public string LeantimeUrl 
        { 
            get => _leantime_url;
            set
            {
                _leantime_url = value;
                SaveToSecureStorage("Leantime_Url", value);
            }
        }
        public string LeantimeApiKey 
        { 
            get => _leantime_api_key;
            set
            {
                _leantime_api_key = value;
                SaveToSecureStorage("Leantime_Api_Key", value);
            }
        }
        public string ChatGPTApiKey
        {
            get => _chatgpt_api_key;
            set
            {
                _chatgpt_api_key = value;
                SaveToSecureStorage("ChatGPT_Api_Key", value);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task SaveToSecureStorage(string key, string value)
        {
            try
            {
                Debug.WriteLine($"SAVING SETTING: {key} - {value}");
                await SecureStorage.SetAsync(key, value);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task LoadSettings()
        {
            _jira_project_key = await SecureStorage.GetAsync("Jira_Project_Key") ?? "";
            _jira_email = await SecureStorage.GetAsync("Jira_Email") ?? "";
            _jira_link = await SecureStorage.GetAsync("Jira_Link") ?? "";
            _jira_api_key = await SecureStorage.GetAsync("Jira_Api_Key") ?? "";
            _leantime_url = await SecureStorage.GetAsync("Leantime_Url") ?? "";
            _leantime_api_key = await SecureStorage.GetAsync("Leantime_Api_Key") ?? "";
            _chatgpt_api_key = await SecureStorage.GetAsync("ChatGPT_Api_Key") ?? "";

            var storyPointsString = await SecureStorage.GetAsync("Story_Points");

            if (bool.TryParse(storyPointsString, out var storyPoints))
                _storyPoints = storyPoints;
            else
                _storyPoints = false; // default

        }

        protected void OnPropertyChanged(string propertyName)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

    }
}
