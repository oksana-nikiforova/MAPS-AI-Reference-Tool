using MAPSAI.Models;

namespace MAPSAI.Views.ProjectScope;

public partial class SectionListener : ContentView
{
	public SectionListener()
	{
		InitializeComponent();
	}

    private readonly Dictionary<ViewChange, Action> _eventHandlers = new();

    public static readonly BindableProperty EventsProperty =
        BindableProperty.Create(
        nameof(Events),
        typeof(List<ViewChange>),
        typeof(WindowListenerView),
        default(List<ViewChange>),
        propertyChanged: EventsChanged);

    public List<ViewChange> Events
    {
        get => (List<ViewChange>)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }

    public List<View> Views { get; set; } =
        [
            new ProjectScopeView() { IsVisible = false, AutomationId = "0", HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand },
            new ProjectScopeSpecificView() { IsVisible = false, AutomationId = "1", HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand },
            new StakeholdersView() { IsVisible = false, AutomationId = "2", HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand },
            new BudgetView() { IsVisible = false, AutomationId = "3", HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand },
            new TeamView() { IsVisible = false, AutomationId = "4", HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand },
        ];


    private static void EventsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SectionListener)bindable;

        if (oldValue is List<ViewChange> oldEvents)
        {
            foreach (var ev in oldEvents)
            {
                if (control._eventHandlers.TryGetValue(ev, out var handler))
                {
                    ev.Fired -= handler;
                    control._eventHandlers.Remove(ev);
                }
            }
        }

        if (newValue is List<ViewChange> newEvents)
        {
            foreach (var ev in newEvents)
            {
                Action handler = () => control.OnEventFired(ev);
                ev.Fired += handler;
                control._eventHandlers[ev] = handler;

                //var view = new MermaidView() { IsVisible = false };

                //view.HorizontalOptions = LayoutOptions.FillAndExpand;
                //view.VerticalOptions = LayoutOptions.FillAndExpand;

                //control.Views.Add(view);
                //control.EventContentStack.Children.Add(view);
            }

            foreach (var view in control.Views)
            {
                view.SetValue(Grid.RowProperty, 0);
                view.SetValue(Grid.ColumnProperty, 0);

                control.EventContentStack.Children.Add(view);
            }

            if (control.Views.Count > 0)
            {
                control.Views[0].IsVisible = true;
            }
        }
    }

    private void OnEventFired(ViewChange ev)
    {
        foreach (var item in Views)
        {
            item.IsVisible = false;
        }

        var matchingView = Views.FirstOrDefault(v => v.AutomationId == ev.Id);

        if (matchingView != null)
        {
            matchingView.IsVisible = true;
        }
    }
}