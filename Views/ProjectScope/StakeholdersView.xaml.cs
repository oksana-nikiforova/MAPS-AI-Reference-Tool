using MAPSAI.Models;

namespace MAPSAI.Views.ProjectScope;

public partial class StakeholdersView : ContentView
{
	public StakeholdersView()
	{
		InitializeComponent();
        BindingContext = DataStore.Instance.Project;

        DataStore.Instance.ProjectChanged += (_, _) =>
        {
            BindingContext = DataStore.Instance.Project;
        };
    }
}