using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics;
using MAPSAI.Core.Models;

namespace MAPSAI.Models
{
    public class DataStore: INotifyPropertyChanged
    {
        private DataStore() 
        {
            //ActionsRunner.Instance.ProjectPlanUploaded += AddNewPlan;
        }

        private static readonly Lazy<DataStore> lazy = new(() => new DataStore());
        public static DataStore Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public UserStory SelectedStory { get; set; }

        // LOGS
        public ObservableCollection<string> Logs { get; set; } = [];

        public void AddLog(string log)
        {
            log = "[" + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss") + "] " + log;
            Logs.Add(log);
            ActionsRunner.Instance.NotifyLogsChanged();
        }

        //PROJECT INSTANCE
        public Project Project { get; set; } = new();
        public event EventHandler? ProjectChanged;

        public virtual void OnProjectChanged()
        {
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        private Standard _activeStandard { get; set; }

        public Standard ActiveStandard
        {
            get => _activeStandard;
            set
            {
                _activeStandard = value;
                OnPropertyChanged();
            }
        }

        //SAVING FUNCTIONALITY
        public void Save(string filePath = "project.json")
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            var saveData = new SaveData
            {
                Project = this.Project,
            };

            File.WriteAllText(filePath, JsonSerializer.Serialize(saveData, options));
        }

        public void Load(string filePath = "project.json")
        {
            if (File.Exists(filePath))
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    IncludeFields = true
                };

                var loaded = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(filePath), options);

                if (loaded != null)
                {
                    this.Project = loaded.Project ?? new Project();

                    Debug.WriteLine($"Standard count: {this.Project.Standards.Count}");

                    foreach (var standard in this.Project.Standards)
                    {
                        foreach (var root in standard.Tree)
                        {
                            RestoreParents(root);
                        }
                    }
                }

                ActionsRunner.Instance.NotifySavedProjectLoaded();
                ActionsRunner.Instance.NotifyUserStoriesChanged();
            }
        }

        public static void RestoreParents<T>(TreeNode<T> node, TreeNode<T> parent = null)
        {
            if (node == null) return;

            node.Parent = parent;

            foreach (var child in node.Children)
            {
                RestoreParents(child, node);
            }
        }

    }
}