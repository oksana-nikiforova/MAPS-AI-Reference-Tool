using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using MAPSAI.Models;
using MAPSAI.Services.AI;
using System.Diagnostics;

namespace MAPSAI.Views.Backlog;

public partial class ProcessingPopup : Popup
{
    private ListEntryService _listEntryService;

    private CancellationTokenSource _cts = new();

    public ProcessingPopup()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        ((Button)sender).IsEnabled = false;

        InfoLabel.Text = "Cancelling, please wait...";
        CancelIndicator.IsVisible = true;
        CancelIndicator.IsRunning = true;

        _cts.Cancel();
    }

    private async void OnLoaded(object sender, EventArgs e)
    {
        _listEntryService = Handler?.MauiContext?.Services.GetRequiredService<ListEntryService>();

        await Task.Delay(300);
        await GenerateAllStoryElementsAsync(_cts.Token);
    }

    private async Task GenerateAllStoryElementsAsync(CancellationToken token)
    {
        try
        {
            Debug.WriteLine("Started Processing!");
            var stories = DataStore.Instance.Project.UserStories;
            if (stories.Count == 0) return;

            int total = stories.Count;
            int processed = 0;

            foreach (var userStory in stories)
            {
                token.ThrowIfCancellationRequested();
                processed++;

                InfoLabel.Text = $"Processing story {processed} of {total}...";
                ProgressBar.Progress = (double)processed / total;
                DataStore.Instance.SelectedStory = userStory;
                ProgressLabel.Text = "Generating Priority...";
                userStory.Priority = await userStory.GeneratePriority(_listEntryService);
                token.ThrowIfCancellationRequested();

                ProgressLabel.Text = "Generating Reason...";
                await userStory.Purpose.GenerateAsync(_listEntryService);
                token.ThrowIfCancellationRequested();
                ProgressLabel.Text = "Generating Acceptance Criteria...";
                await userStory.AcceptanceCriteria.GenerateAsync(_listEntryService);
                token.ThrowIfCancellationRequested();
            }

            InfoLabel.Text = "All user stories processed successfully!";
            ProgressBar.Progress = 1.0;

            await Task.Delay(1500);

            await Shell.Current.ClosePopupAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = "Failed to generate!";
            Debug.WriteLine(ex.ToString());
            await Shell.Current.ClosePopupAsync();
        }
    }
}
