using MAPSAI.Models;

namespace MAPSAI.Pages;

public partial class ProjectDataFilesPage : ContentPage
{

	public ProjectDataFilesPage()
	{
        InitializeComponent();
        BindingContext = this;
        ActionsRunner.Instance.ProjectFilesUpload += OnProjectFilesUpload;
        ActionsRunner.Instance.UserStoriesChanged += OnProjectFilesUpload;
    }

    public List<ViewChange> Actions { get; set; } = new()
    {
        new("Business Process Model", "PROCESS"),
        new("Use Case Diagram", "USE_CASE")
    };

    public void OnProjectFilesUpload()
	{
        ListenerView.RebuildDiagram();
    }

    protected override void OnAppearing()
    {
        ListenerView.RebuildDiagram();
    }
}