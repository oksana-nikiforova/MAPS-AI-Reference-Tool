using MAPSAI.Models;

namespace MAPSAI.Pages;

public partial class ProjectScopePage : ContentPage
{
    public ProjectScopePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public List<ViewChange> Actions { get; set; } = new()
    {
        new("Project Scope", ""){ Id = "0" },
        new("Project Scope Specific", ""){ Id = "1" },
        new("Stakeholders", ""){ Id = "2" },
        //new("Budget", ""){ Id = "3" },
        //new("Team", ""){ Id = "4" }
    };
}