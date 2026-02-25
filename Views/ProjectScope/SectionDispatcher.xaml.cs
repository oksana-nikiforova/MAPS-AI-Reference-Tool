using MAPSAI.Models;
using MAPSAI.Platforms.Windows;
using MAPSAI.Services;

namespace MAPSAI.Views.ProjectScope;

public partial class SectionDispatcher : ContentView
{
	public SectionDispatcher()
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

    private static void EventsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SectionDispatcher)bindable;
        var events = newValue as List<ViewChange>;

        if (events != null)
        {
            control.EventButtonStack.Clear();

            foreach (var item in events)
            {
                var button = new Button
                {
                    Text = item.Name,
                    Style = (Style)Application.Current.Resources["DefaultButtonStyle"]
                };

                CursorBehavior.SetCursor(button, CursorIcon.Hand);

                button.Clicked += (s, e) => control.ButtonPress(item);

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
            if (events.Count > 0)
                events[0].IsActive = true;
        }
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