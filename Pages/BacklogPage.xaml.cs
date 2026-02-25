using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui;
using MAPSAI.Models;
using MAPSAI.Views.Backlog;
using System.Diagnostics;
using MAPSAI.Services.AI;
using MAPSAI.Core.Models;
using MAPSAI.Models.AI;

namespace MAPSAI.Pages;

public partial class BacklogPage : ContentPage
{
	public BacklogPage()
	{
        InitializeComponent();
		BindingContext = this;
        ActionsRunner.Instance.StorySelected += OnStorySelected;

        Init();
    }

    private async void Init()
    {
        ActionsRunner.Instance.GenerateAllBacklog += Instance_GenerateAllBacklog;
    }

    private async void Instance_GenerateAllBacklog()
    {
        await ProcessPopup();
    }

    private bool _canRunpopup = true;

    public bool CanRunPopup
    {
        get => _canRunpopup;
        set
        {
            _canRunpopup = value;
            OnPropertyChanged(nameof(CanRunPopup));
        }
    }

    private UserStory _selectedStory;

    public UserStory SelectedStory
    {
        get => _selectedStory;
        set
        {
            _selectedStory = value;
            OnPropertyChanged(nameof(SelectedStory));
        }
    }

    private void OnStorySelected(object? sender, UserStory e)
    {
        SelectedStory = e;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        Debug.WriteLine("Navigated back to BacklogPage");

        if (DataStore.Instance.Project.UserStories.Count > 0)
        {
            foreach (var story in DataStore.Instance.Project.UserStories)
            {
                story.IsSelected = false;
            }
            SelectedStory = DataStore.Instance.Project.UserStories[0];
            DataStore.Instance.Project.UserStories[0].IsSelected = true;
            DataStore.Instance.SelectedStory = SelectedStory;
            Debug.WriteLine($"Selected: {SelectedStory}");
        }
    }

    private async Task ProcessPopup()
    {
        if (DataStore.Instance.Project.UserStories.Count == 0) return;

        if (!CanRunPopup) return;

        CanRunPopup = false;

        try
        {
            var popup = new ProcessingPopup();

            await Application.Current.MainPage.ShowPopupAsync(popup, new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Popup failed: {ex}");
        }
        finally
        {
            CanRunPopup = true;
        }
    }

}