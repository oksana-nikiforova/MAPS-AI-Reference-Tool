using MAPSAI.Models;

namespace MAPSAI.Views.ProjectScope;

public partial class ProjectScopeSpecificView : ContentView
{
	public ProjectScopeSpecificView()
	{
		InitializeComponent();
        BindingContext = DataStore.Instance.Project;

        DataStore.Instance.ProjectChanged += (_, _) =>
        {
            BindingContext = DataStore.Instance.Project;
        };
    }

}