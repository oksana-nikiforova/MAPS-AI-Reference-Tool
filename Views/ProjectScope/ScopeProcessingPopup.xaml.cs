using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using MAPSAI.Core.Models;
using MAPSAI.Models.AI;
using MAPSAI.Models;
using MAPSAI.Services.AI;
using System.Diagnostics;
using System.Text.Json;
using MAPSAI.Services.Builders;

namespace MAPSAI.Views.ProjectScope;

public partial class ScopeProcessingPopup : Popup
{
    private ListEntryService _listEntryService;

    private CancellationTokenSource _cts = new();

    public ScopeProcessingPopup()
    {
		InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        // Disable cancel button immediately
        ((Button)sender).IsEnabled = false;

        // Switch UI into "cancelling" mode
        InfoLabel.Text = "Cancelling, please wait...";
        CancelIndicator.IsVisible = true;
        CancelIndicator.IsRunning = true;

        // Cancel the token
        _cts.Cancel();
    }

    private async void OnLoaded(object sender, EventArgs e)
    {
        _listEntryService = Handler?.MauiContext?.Services.GetRequiredService<ListEntryService>();

        await Task.Delay(300);
        await GenerateAllScopeElementsAsync(_cts.Token);
    }

    private async Task GenerateAllScopeElementsAsync(CancellationToken token)
    {
        try
        {
            Debug.WriteLine("Started Processing!");
            var stories = DataStore.Instance.Project.UserStories;
            if (stories.Count == 0) return;

            int total = 7;
            int processed = 0;

            processed++;
            InfoLabel.Text = $"Processing scope element {processed} of {total}...";
            ProgressBar.Progress = (double)processed / total;

            ProgressLabel.Text = "Generating Project Purpose...";
            await DataStore.Instance.Project.ProjectPurpose.GenerateAsync(_listEntryService);
            processed++;
            InfoLabel.Text = $"Processing scope element {processed} of {total}...";
            ProgressBar.Progress = (double)processed / total;
            token.ThrowIfCancellationRequested();

            ProgressLabel.Text = "Generating Background...";
            await DataStore.Instance.Project.Background.GenerateAsync(_listEntryService);
            processed++;
            InfoLabel.Text = $"Processing scope element {processed} of {total}...";
            ProgressBar.Progress = (double)processed / total;
            token.ThrowIfCancellationRequested();

            ProgressLabel.Text = "Generating Deliverables...";
            await DataStore.Instance.Project.Deliverables.GenerateAsync(_listEntryService);
            processed++;
            InfoLabel.Text = $"Processing scope element {processed} of {total}...";
            ProgressBar.Progress = (double)processed / total;
            token.ThrowIfCancellationRequested();

            ProgressLabel.Text = "Generating Resource Requirements...";
            await DataStore.Instance.Project.ResourceRequirements.GenerateAsync(_listEntryService);
            processed++;
            InfoLabel.Text = $"Processing scope element {processed} of {total}...";
            ProgressBar.Progress = (double)processed / total;
            token.ThrowIfCancellationRequested();

            ProgressLabel.Text = "Generating Operations and Support...";
            await DataStore.Instance.Project.OperationsSupport.GenerateAsync(_listEntryService);
            processed++;
            InfoLabel.Text = $"Processing scope element {processed} of {total}...";
            ProgressBar.Progress = (double)processed / total;
            token.ThrowIfCancellationRequested();

            ProgressLabel.Text = "Generating Safety and Security...";
            await DataStore.Instance.Project.SafetySecurity.GenerateAsync(_listEntryService);
            processed++;
            InfoLabel.Text = $"Processing scope element {processed} of {total}...";
            ProgressBar.Progress = (double)processed / total;
            token.ThrowIfCancellationRequested();

            ProgressLabel.Text = "Generating Stakeholders...";
            await DataStore.Instance.Project.Stakeholders.GenerateAsync(_listEntryService);


            InfoLabel.Text = "All scope elements processed successfully!";
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