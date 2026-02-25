using System.ComponentModel;

namespace MAPSAI.Views;

public partial class NavigationView : ContentView, INotifyPropertyChanged
{
	public NavigationView()
	{
		InitializeComponent();
        BindingContext = this;
        BuildNavigation();

        Shell.Current.Navigated += OnShellNavigated;
    }

    protected void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
    {
        if (Shell.Current?.CurrentPage != null)
        {
            ActivePage = Shell.Current.CurrentState.Location.ToString();
        }
    }

    public Dictionary<string, string> Routes { get; set; } = new()
    {
        { "//Overview", "Domain" },
        { "//Backlog", "Backlog" },
        { "//Project_scope", "Scope" },
        { "//Plan", "Plan" },
    };

    private string _activePage = "";
    public string ActivePage 
    { 
        get => _activePage;
        set
        {
            if (_activePage != value)
            {
                _activePage = value;
                OnPropertyChanged(nameof(ActivePage));
            }
        }
    }

    private async void BuildNavigation()
    {
        foreach (var route in Routes)
        {
            var button = new Button()
            {
                Text = route.Value,
                CommandParameter = route.Key,
                Style = (Style)Application.Current.Resources["NavigationButtonStyle"]
            };

            button.Clicked += Navigate;

            var trigger = new DataTrigger(typeof(Button))
            {
                Binding = new Binding("ActivePage"),
                Value = route.Key
            };
            trigger.Setters.Add(new Setter
            {
                Property = Button.BackgroundProperty,
                Value = Application.Current.Resources["PrimaryDark"]
            });

            button.Triggers.Add(trigger);

            NaviagtionStack.Children.Add(button);
        }
    }

    private async void Navigate(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string parameter)
        {
            await Shell.Current.GoToAsync(parameter, true);
        }
    }
}