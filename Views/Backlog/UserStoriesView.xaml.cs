using MAPSAI.Core.Models;
using MAPSAI.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MAPSAI.Views.Backlog;

public class UserStoryGroup
{
    public string Key { get; set; } = string.Empty;
    public ObservableCollection<UserStory> Stories { get; set; } = new();
}

public partial class UserStoriesView : ContentView
{
    public UserStoriesView()
	{
		InitializeComponent();
        BindingContext = this;
        ProcessUserStoriesInBackground();

        ActionsRunner.Instance.UserStoriesChanged += async () =>
        {
            await RefreshAsync();
        };

        HaveStories = DataStore.Instance.Project.UserStories.Count > 0;
    }

    private bool _haveStories;
    public bool HaveStories
    {
        get => _haveStories;
        set
        {
            _haveStories = value;
            OnPropertyChanged(nameof(HaveStories));
        }
    }

    public ObservableCollection<UserStoryGroup> UserStoryGroups { get; set; } = [];

    public async Task RefreshAsync()
    {
        HaveStories = DataStore.Instance.Project.UserStories.Count > 0;
        await Task.Run(ProcessUserStoriesInBackground);
    }

    private void ProcessUserStoriesInBackground()
    {
        try
        {
            var projectStories = DataStore.Instance.Project.UserStories;

            if (projectStories.Count == 0)
            {
                return;
            }

            var tempDict = new Dictionary<string, ObservableCollection<UserStory>>();

            foreach (var item in projectStories)
            {
                if (!tempDict.TryGetValue(item.User, out var list))
                {
                    list = new ObservableCollection<UserStory>();
                    tempDict[item.User] = list;
                }
                list.Add(item);
            }

            var newGroups = tempDict.Select(kvp => new UserStoryGroup
            {
                Key = kvp.Key,
                Stories = kvp.Value
            }).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UserStoryGroups.Clear();
                foreach (var g in newGroups)
                    UserStoryGroups.Add(g);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private void OnStoryTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label && label.BindingContext is UserStory story)
        {
            DataStore.Instance.SelectedStory = story;
            ActionsRunner.Instance.NotifyStorySelected(story);
            Debug.WriteLine($"Tapped: {story.Story}");

            foreach (var group in UserStoryGroups)
            {
                foreach (var item in group.Stories)
                {
                    item.IsSelected = false;
                    if (item.User == DataStore.Instance.SelectedStory.User && item.Story == DataStore.Instance.SelectedStory.Story)
                    {
                        item.IsSelected = true;
                    }
                }
            }
        }
    }

    private void OnGenerateClicked(object sender, EventArgs e)
    {
        ActionsRunner.Instance.NotifyGenerateAllBacklog();
    }
}