

using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui;
using MAPSAI.Models;
using MAPSAI.Services.AI;
using MAPSAI.Services.Builders;
using System.Diagnostics;
using MAPSAI.Core.Models;
using MAPSAI.Models.AI;

namespace MAPSAI.Views.ProjectScope;

public partial class OverAllInfo : ContentView
{

	public OverAllInfo()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await GenerateScope();
    }

    public async Task GenerateScope()
	{
        if (DataStore.Instance.Project.UserStories.Count == 0) return;

        try
        {
            var popup = new ScopeProcessingPopup();

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
            Debug.WriteLine($"Operation done!");
        }

    }
}