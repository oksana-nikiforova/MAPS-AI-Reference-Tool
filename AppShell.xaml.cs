using CommunityToolkit.Maui.Storage;
using MAPSAI.Models;
using MAPSAI.Pages;
using MAPSAI.Services;
using MAPSAI.Services.Builders;
using MAPSAI.Services.Files;
using System.Diagnostics;
using System.Text;

namespace MAPSAI
{
    public partial class AppShell : Shell
    {
        private MSWordService _wordService;
        private ProcessModelBuilder _processModelBuilder;
        private PlantUMLProcessModelBuilder _processModelBuilderPlantUML;
        private UseCaseBuilder _useCaseBuilder;
        private PlantUMLBuilder _useCaseBuilderPlantUML;
        private ExcelService _excelService;

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(BacklogPage), typeof(BacklogPage));
            Routing.RegisterRoute(nameof(ProjectDataFilesPage), typeof(ProjectDataFilesPage));
            Routing.RegisterRoute(nameof(ProjectScopePage), typeof(ProjectScopePage));
            Routing.RegisterRoute(nameof(PlanPage), typeof(PlanPage));
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler?.MauiContext != null)
            {
                _excelService = Handler.MauiContext.Services.GetRequiredService<ExcelService>();
                _wordService = Handler.MauiContext.Services.GetRequiredService<MSWordService>();
                _processModelBuilder = Handler.MauiContext.Services.GetRequiredService<ProcessModelBuilder>();
                _processModelBuilderPlantUML = Handler.MauiContext.Services.GetRequiredService<PlantUMLProcessModelBuilder>();
                _useCaseBuilder = Handler.MauiContext.Services.GetRequiredService<UseCaseBuilder>();
                _useCaseBuilderPlantUML = Handler.MauiContext.Services.GetRequiredService<PlantUMLBuilder>();
            }
        }

        private async void OnNewClicked(object sender, EventArgs e)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "New Project",
                "Create a new project? Unsaved changes will be lost.",
                "Yes",
                "Cancel");

            if (!confirm)
                return;

            DataStore.Instance.Project = new Project();
            DataStore.Instance.OnProjectChanged();
            DataStore.Instance.SelectedStory = new();

            ActionsRunner.Instance.NotifyUserStoriesChanged();

            await Shell.Current.GoToAsync("//Overview");
        }

        private async void OnOpenClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Open Project",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".json" } },
            { DevicePlatform.MacCatalyst, new[] { "json" } }
        })
            });

            if (result == null)
                return;

            try
            {
                DataStore.Instance.Load(result.FullPath);
                DataStore.Instance.OnProjectChanged();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Failed to load project:\n{ex.Message}",
                    "OK");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var title = string.IsNullOrWhiteSpace(DataStore.Instance.Project.Title)
                ? "no_title"
                : DataStore.Instance.Project.Title;

            title = title.Replace(" ", "_").ToLower();

            var result = await FileSaver.Default.SaveAsync(
                $"project_{title}.json",
                new MemoryStream());

            if (!result.IsSuccessful)
                return;

            try
            {
                DataStore.Instance.Save(result.FilePath);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Failed to save project:\n{ex.Message}",
                    "OK");
            }
        }

        private async void OnExportAllClicked(object sender, EventArgs e)
        {
            //diagrams image if possible
            if (!String.IsNullOrWhiteSpace(DataStore.Instance.Project.userStoryDiagram))
            {
                await SaveImageFromBase64Async(DataStore.Instance.Project.userStoryDiagram, "use_case");
            }

            if (!String.IsNullOrWhiteSpace(DataStore.Instance.Project.processDiagram))
            {
                await SaveImageFromBase64Async(DataStore.Instance.Project.processDiagram, "process_model");
            }

            if (!String.IsNullOrWhiteSpace(DataStore.Instance.Project.ganttDiagram))
            {
                await SaveImageFromBase64Async(DataStore.Instance.Project.ganttDiagram, "roadmap");
            }

            //diagram codes
            var useCaseMermaid = _useCaseBuilder.Build(DataStore.Instance.Project.UserStories, true);

            await SaveDiagramText(useCaseMermaid, "use_case", "mermaid");

            var useCasePlant = _useCaseBuilderPlantUML.Build(DataStore.Instance.Project.UserStories);

            await SaveDiagramText(useCasePlant, "use_case", "plantuml");

            var processMermaid = _processModelBuilder.Build(DataStore.Instance.Project.UserStories);

            await SaveDiagramText(processMermaid, "process", "mermaid");

            var processPlant = _processModelBuilderPlantUML.Build(DataStore.Instance.Project.UserStories);

            await SaveDiagramText(processPlant, "process", "plantuml");

            ExportWord(new(), new());

            await _wordService.GenerateDocument(DataStore.Instance.Project.PlanTree, "Project plan", "IT_Plan");

        }


        private async Task SaveDiagramText(string diagramCode, string type, string codeType, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(diagramCode))
            {
                Debug.WriteLine("Diagram code: no data to save.");
                return;
            }

            var fileName = $"{type}_diagram_code_{codeType}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(diagramCode));
            stream.Position = 0;

            var result = await FileSaver.Default.SaveAsync(fileName, stream, token);

            if (result.IsSuccessful)
            {
                Debug.WriteLine($"Diagram code saved to: {result.FilePath}");
                _ = NotificationHelper.ShowSnackbar("Diagram code save successfully!", "Ok");
            }
            else
            {
                Debug.WriteLine($"Diagram code save failed: {result.Exception?.Message}");
                _ = NotificationHelper.ShowSnackbar("Diagram code save failed!", "Ok");
            }
        }

        public Dictionary<string, string> GetScopeDictionary()
        {
            Dictionary<string, string> scopeDict = [];

            scopeDict.Add("Project title", DataStore.Instance.Project.Title);
            scopeDict.Add("Project number", DataStore.Instance.Project.ProjectNumber);
            scopeDict.Add("Purpose of project", DataStore.Instance.Project.ProjectPurpose.Text);
            scopeDict.Add("Purpose background", DataStore.Instance.Project.Background.Text);

            StringBuilder deliverables = new StringBuilder();

            foreach (var item in DataStore.Instance.Project.Deliverables.Collection)
            {
                deliverables.AppendLine(item.Text);
            }

            scopeDict.Add("Deliverables", deliverables.ToString());

            StringBuilder resourceReq = new StringBuilder();

            foreach (var item in DataStore.Instance.Project.ResourceRequirements.Collection)
            {
                resourceReq.AppendLine(item.Text);
            }

            scopeDict.Add("Resource requirements", resourceReq.ToString());

            scopeDict.Add("Operations & Support", DataStore.Instance.Project.OperationsSupport.Text);

            StringBuilder safety = new StringBuilder();

            foreach (var item in DataStore.Instance.Project.SafetySecurity.Collection)
            {
                safety.AppendLine(item.Text);
            }

            scopeDict.Add("Safety, Security & Risks", safety.ToString());


            StringBuilder stakeholders = new StringBuilder();

            foreach (var item in DataStore.Instance.Project.Stakeholders.Collection)
            {
                stakeholders.AppendLine(item.Text);
            }

            scopeDict.Add("Stakeholders", stakeholders.ToString());

            return scopeDict;
        }

        private async void ExportWord(object sender, EventArgs e)
        {
            var scopeDict = GetScopeDictionary();

            //create new Treenode here for scope
            TreeNode<string> scopeTree = new("Root");

            foreach (var item in scopeDict)
            {
                scopeTree.AddChild(new(item.Key), item.Value);
            }

            await _wordService.GenerateDocument(scopeTree.Children, "Project scope document", "scope_artefact");
        }

        private async void ExportExcel(object sender, EventArgs e)
        {
            var scopeDict = GetScopeDictionary();

            await _excelService.DictionaryToExcelAsync(scopeDict, "scope_artefact");
        }

        private async Task SaveImageFromBase64Async(string? base64String, string diaType, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                Debug.WriteLine("SaveImageFromBase64Async: no data to save.");
                return;
            }

            var commaIdx = base64String.IndexOf(',');
            if (base64String.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIdx >= 0)
            {
                base64String = base64String[(commaIdx + 1)..];
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64String);
            }
            catch (FormatException fex)
            {
                Debug.WriteLine($"SaveImageFromBase64Async: invalid Base64. {fex}");
                return;
            }

            var fileName = $"{diaType}_diagram_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            using var stream = new MemoryStream(imageBytes);

            stream.Position = 0;

            var result = await FileSaver.Default.SaveAsync(fileName, stream, token);

            if (result.IsSuccessful)
            {
                Debug.WriteLine($"Image saved to: {result.FilePath}");
                _ = NotificationHelper.ShowSnackbar("Image save successfully!", "Ok");
            }
            else
            {
                Debug.WriteLine($"Image save failed: {result.Exception?.Message}");
                _ = NotificationHelper.ShowSnackbar("Image save failed!", "Ok");
            }
        }

    }
}
