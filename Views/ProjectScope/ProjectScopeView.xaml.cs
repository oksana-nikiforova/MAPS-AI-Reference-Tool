using MAPSAI.Models;
using System.ComponentModel;

namespace MAPSAI.Views.ProjectScope;

public partial class ProjectScopeView : ContentView
{
    public ProjectScopeView()
    {
        InitializeComponent();
        BindingContext = DataStore.Instance.Project;

        DataStore.Instance.ProjectChanged += (_, _) =>
        {
            BindingContext = DataStore.Instance.Project;
        };
    }

}