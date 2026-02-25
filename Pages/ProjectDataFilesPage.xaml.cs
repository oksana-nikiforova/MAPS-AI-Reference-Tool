using MAPSAI.Models;
using MAPSAI.Services.AI;
using System.Diagnostics;
using System.Text;

namespace MAPSAI.Pages;

public partial class ProjectDataFilesPage : ContentPage
{
    private StoryPointService _storyPointsService;

	public ProjectDataFilesPage(StoryPointService storyPointService)
	{
        InitializeComponent();
        BindingContext = this;
        ActionsRunner.Instance.ProjectFilesUpload += OnProjectFilesUpload;
        ActionsRunner.Instance.UserStoriesChanged += OnProjectFilesUpload;
        _storyPointsService = storyPointService;

        TestResponse();
    }

    public async void TestResponse()
    {
        await _storyPointsService.InitializeAsync();
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