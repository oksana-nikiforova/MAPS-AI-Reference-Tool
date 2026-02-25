
using DocumentFormat.OpenXml.Presentation;
using MAPSAI.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MAPSAI.Models
{
    public class Project: INotifyPropertyChanged
    {
        public Project()
        {
            ActionsRunner.Instance.ProjectPlanUploaded += AddNewPlan;
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _projectnumber = string.Empty;
        public string ProjectNumber
        {
            get => _projectnumber;
            set
            {
                if (_projectnumber != value)
                {
                    _projectnumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Standard> Standards { get; set; } = [];

        private Standard _activeStandard;
        public Standard ActiveStandard
        {
            get => _activeStandard;
            set
            {
                if (_activeStandard != value)
                {
                    _activeStandard = value;
                    OnPropertyChanged();
                }
            }
        }

        public void AddNewPlan(object? sender, Standard standard)
        {
            Standards.Add(standard);
        }

        public ObservableCollection<TreeNode<string>> PlanTree { get; set; }

        public FileInfo BusinessProcessModelFile { get; set; } = new();
        public FileInfo BusinessProcessModelExcelFile { get; set; } = new();
        public ObservableCollection<FileInfo> Files { get; set; } = [];

        public Dictionary<string, string> PlanDictionary { get; set; } = [];

        public string userStoryDiagram { get; set; } = "";
        public string processDiagram { get; set; } = "";
        public string ganttDiagram { get; set; } = "";

        public ObservableCollection<UserStory> UserStories { get; set; } = new ObservableCollection<UserStory>();
        public GeneratableEntry ProjectPurpose { get; set; } = new("Purpose of project:", "high-level narrative regarding what this project is expected to accomplish & its benefits", "DefaultEntryPrompt", "DefaultEntryMetaPrompt", 192);
        public GeneratableEntry Background { get; set; } = new("Background:", "brief narrative regarding what led to this project proposal", "DefaultEntryPrompt", "DefaultEntryMetaPrompt", 192);
        public ListItem<ListElement> Deliverables { get; set; } = new("Deliverable", "DeliverablesPrompt", "a sampling of key deliverables", 384);
        public ListItem<ListElement> Stakeholders { get; set; } = new("Stakeholder", "StakeholdersPrompt", "those involved in or who may be affected by project activities", 384);
        public ListItem<ListElement> ResourceRequirements { get; set; } = new("Resource requirements", "ResourceReqPrompt", "Resources likely to be required to deliver the project, including human roles, technical assets, and organizational support.", 384);
        public GeneratableEntry OperationsSupport { get; set; } = new("Operations & Support:", "define product ownership and who is responsible for product maintenance & support", "DefaultEntryPrompt", "DefaultEntryMetaPrompt", 192);
        public ListItem<ListElement> SafetySecurity { get; set; } = new("Safety, Security & Risks", "SafetySecurityPrompt", "", 384);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
