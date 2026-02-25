using MAPSAI.Models;
using MAPSAI.Platforms.Windows;
using MAPSAI.Services;
using System.Diagnostics;

namespace MAPSAI.Views;

public partial class EventDispatcherView : ContentView
{
	public EventDispatcherView()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty EventsProperty =
    BindableProperty.Create(
        nameof(Events),
        typeof(List<ViewChange>),
        typeof(EventDispatcherView),
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
            typeof(EventDispatcherView),
            default(string),
            propertyChanged: OnDisplayPropertyChanged);

    public string DisplayProperty
    {
        get => (string)GetValue(DisplayPropertyProperty);
        set => SetValue(DisplayPropertyProperty, value);
    }

    private static void OnDisplayPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (EventDispatcherView)bindable;
        if (control.TitleLabel != null)
        {
            control.TitleLabel.Text = newValue as string;
        }
    }

    private static void EventsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (EventDispatcherView)bindable;
        var events = newValue as List<ViewChange>;

        if (events == null)
            return;

        control.EventButtonStack.Clear();

        foreach (var item in events)
        {
            var button = new Button
            {
                Text = item.Name,
                Style = (Style)Application.Current.Resources["DefaultButtonStyle"]
            };

            CursorBehavior.SetCursor(button, CursorIcon.Hand);

            // Initial color based on IsActive
            button.BackgroundColor = item.IsActive ? Colors.Red : (Color)Application.Current.Resources["Primary"];

            // Handle clicks
            button.Clicked += (s, e) => control.ButtonPress(item);

            // Listen for IsActive changes
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewChange.IsActive))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        VisualStateManager.GoToState(button, item.IsActive ? "Active" : "Inactive");
                    });
                }
            };

            // Set initial state when creating the button
            VisualStateManager.GoToState(button, item.IsActive ? "Active" : "Inactive");

            control.EventButtonStack.Add(button);
        }

        // Optionally mark first as active
        if (events.Count > 0)
            events[0].IsActive = true;
    }

    private void ButtonPress(ViewChange item)
    {
        foreach (var item1 in Events)
        {
            item1.IsActive = false;
        }

        item.Raise();
    }
}