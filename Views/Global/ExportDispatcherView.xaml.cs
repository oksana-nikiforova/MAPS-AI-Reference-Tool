using MAPSAI.Models;
using MAPSAI.Views.UML;

namespace MAPSAI.Views;

public partial class ExportDispatcherView : ContentView
{
	public ExportDispatcherView()
	{
		InitializeComponent();
	}

    public List<ExportView> Views { get; set; } = [];

    private readonly Dictionary<ViewChange, Action> _eventHandlers = new();

    public static readonly BindableProperty EventsProperty =
    BindableProperty.Create(
        nameof(Events),
        typeof(List<ViewChange>),
        typeof(ExportDispatcherView),
        default(List<ViewChange>),
        propertyChanged: EventsChanged);

    public List<ViewChange> Events
    {
        get => (List<ViewChange>)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }

    public static readonly BindableProperty DisplayPropertyProperty =
        BindableProperty.Create(
            nameof(DisplayProperty),
            typeof(string),
            typeof(ExportDispatcherView),
            default(string),
            propertyChanged: OnDisplayPropertyChanged);

    public string DisplayProperty
    {
        get => (string)GetValue(DisplayPropertyProperty);
        set => SetValue(DisplayPropertyProperty, value);
    }

    private static void OnDisplayPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ExportDispatcherView)bindable;
        //if (control.TitleLabel != null)
        //{
        //    control.TitleLabel.Text = newValue as string;
        //}
    }

    private static void EventsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ExportDispatcherView)bindable;

        if (control.ExportContentStack.Children.Count > 0) return;

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

                    var view = new ExportView() { RequestComponent = ev.DiagramType, ExportTitle = $"Export {ev.Name} as" , IsVisible = false };

                    view.SetValue(Grid.RowProperty, 0);
                    view.SetValue(Grid.ColumnProperty, 0);

                    view.HorizontalOptions = LayoutOptions.FillAndExpand;
                    view.VerticalOptions = LayoutOptions.FillAndExpand;

                    control.Views.Add(view);
                    control.ExportContentStack.Children.Add(view);
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

        var matchingView = Views.FirstOrDefault(v => v.RequestComponent == ev.DiagramType);

        if (matchingView != null)
        {
            matchingView.IsVisible = true;
        }
    }
}