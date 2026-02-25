using MAPSAI.Models;
using MAPSAI.Views.UML;

namespace MAPSAI.Views;

public partial class WindowListenerView : ContentView
{
	public WindowListenerView()
	{
		InitializeComponent();
	}

    private readonly Dictionary<ViewChange, Action> _eventHandlers = new();

    public List<MermaidView> Views { get; set; } = [];

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

    private static void EventsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (WindowListenerView)bindable;

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

                var view = new MermaidView() { DiagramType = ev.DiagramType, IsVisible = false };

                view.SetValue(Grid.RowProperty, 0);
                view.SetValue(Grid.ColumnProperty, 0);

                view.HorizontalOptions = LayoutOptions.FillAndExpand;
                view.VerticalOptions = LayoutOptions.FillAndExpand;

                control.Views.Add(view);
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

        var matchingView = Views.FirstOrDefault(v => v.DiagramType == ev.DiagramType);

        if (matchingView != null)
        {
            matchingView.IsVisible = true;
            matchingView.BuildDiagram();
        }
    }

    public void RebuildDiagram()
    {
        foreach (var item in Views)
        {
            if (item.IsVisible)
            {
                item.BuildDiagram();
            }
        }
    }
}