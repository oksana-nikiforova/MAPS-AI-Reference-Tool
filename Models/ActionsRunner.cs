
using MAPSAI.Core.Models;
using MAPSAI.Views.UML;

namespace MAPSAI.Models
{
    public class ActionsRunner
    {
        private ActionsRunner() { }

        private static readonly Lazy<ActionsRunner> lazy = new(() => new ActionsRunner());
        public static ActionsRunner Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public event Action? UserStoriesChanged;
        public void NotifyUserStoriesChanged() => UserStoriesChanged?.Invoke();

        public event Action? LogsChanged;
        public void NotifyLogsChanged() => LogsChanged?.Invoke();

        public event Action? ProjectFilesUpload;
        public void NotifyFilesUpload() => ProjectFilesUpload?.Invoke();

        public event Action<ExportRequest>? ExportDiagramRequest;
        public void NotifyExportDiagramRequest(ExportRequest export_request) => ExportDiagramRequest?.Invoke(export_request);

        public event EventHandler<UserStory>? StorySelected;
        public void NotifyStorySelected(UserStory story) => StorySelected?.Invoke(this, story);

        public event EventHandler<Standard>? ProjectPlanUploaded;
        public void NotifyProjectPlanUploaded(Standard newStandard) => ProjectPlanUploaded?.Invoke(this, newStandard);

        public event EventHandler<Standard>? ProjectPlanSelected;
        public void NotifyProjectPlanSelected(Standard selectedStandard) => ProjectPlanSelected?.Invoke(this, selectedStandard);

        public event EventHandler<Standard>? ProjectPlanDeleted;
        public void NotifyProjectPlanDeleted(Standard deletedStandard) => ProjectPlanDeleted?.Invoke(this, deletedStandard);

        public event Action? StandardParsingDone;
        public void NotifyStandardParsingDone() => StandardParsingDone?.Invoke();

        public event Action? SavedProjectLoaded;
        public void NotifySavedProjectLoaded() => SavedProjectLoaded?.Invoke();

        public event Action? GenerateAllBacklog;
        public void NotifyGenerateAllBacklog() => GenerateAllBacklog?.Invoke();
    }
}
