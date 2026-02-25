using MAPSAI.Core.Models;
using MAPSAI.Models;
using MAPSAI.Services.AI;
using MAPSAI.Services.Builders;
using System.Diagnostics;
using System.Text.Json;

namespace MAPSAI.Views.Backlog;

public class PriorityResult
{
    public string priority { get; set; }
}

public partial class UserStoryEditView : ContentView
{
  
    private StoryPointService _storyPointService;
    private ListEntryService _listEntryService;

    public static readonly BindableProperty SelectedStoryProperty =
       BindableProperty.Create(
           nameof(SelectedStory),
           typeof(UserStory),
           typeof(UserStoryEditView),
           propertyChanged: OnSelecterStoryChanged);

    private static void OnSelecterStoryChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is UserStoryEditView view)
        {
            view.Opacity = 0;
            view.FadeTo(1, 1000);
        }
    }

    public UserStory SelectedStory
    {
        get => (UserStory)GetValue(SelectedStoryProperty);
        set => SetValue(SelectedStoryProperty, value);
    }

    public static readonly BindableProperty IsGeneratingProperty =
    BindableProperty.Create(
        nameof(IsGenerating),
        typeof(bool),
        typeof(UserStoryEditView),
        false);

    public bool IsGenerating
    {
        get => (bool)GetValue(IsGeneratingProperty);
        set => SetValue(IsGeneratingProperty, value);
    }

    public static readonly BindableProperty IsWorkingProperty =
    BindableProperty.Create(
        nameof(IsWorking),
        typeof(bool),
        typeof(UserStoryEditView),
        true);

    public bool IsWorking
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public UserStoryEditView()
	{
		InitializeComponent();
        this.Opacity = 0;
        this.FadeTo(1, 2500);
    }

    public bool StoryPoints
    {
        get => SettingsModel.Instance.StoryPoints;
        set
        {
            if (SettingsModel.Instance.StoryPoints == value)
                return;

            SettingsModel.Instance.StoryPoints = value;
            OnPropertyChanged(nameof(StoryPoints));
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        SettingsModel.Instance.PropertyChanged += (_, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
                OnPropertyChanged(nameof(StoryPoints)));
        };
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler?.MauiContext != null)
        {
            _storyPointService = Handler.MauiContext.Services.GetRequiredService<StoryPointService>();
            _listEntryService = Handler.MauiContext.Services.GetRequiredService<ListEntryService>();
        }
    }

    private void OnUserEntryCompleted(object sender, EventArgs e)
    {
        ActionsRunner.Instance.NotifyUserStoriesChanged();
    }

    private async void GeneratePriority(object sender, EventArgs e)
    {
        try
        {
            if (SelectedStory == null) return;
            IsGenerating = true;
            IsWorking = false;

            SelectedStory.Priority = await SelectedStory.GeneratePriority(_listEntryService);

            IsGenerating = false;
            IsWorking = true;
        }
        catch (Exception ex) 
        {
            Debug.WriteLine(ex.Message);
            IsGenerating = false;
            IsWorking = true;
        }
    }

    private async void GenerateStoryPoints(object sender, EventArgs e)
    {
        try
        {
            if (SelectedStory == null) return;
            IsGenerating = true;
            IsWorking = false;

            SelectedStory.StoryPoints = SelectedStory.GenerateStoryPoints(_storyPointService);

            IsGenerating = false;
            IsWorking = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            IsGenerating = false;
            IsWorking = true;
        }
    }
}