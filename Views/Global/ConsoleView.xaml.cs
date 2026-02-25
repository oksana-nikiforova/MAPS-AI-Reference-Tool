using MAPSAI.Models;
using MAPSAI.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace MAPSAI.Views;

public partial class ConsoleView : ContentView, INotifyPropertyChanged
{
    public ConsoleView()
    {
        InitializeComponent();
        BindingContext = this;

        Logs = DataStore.Instance.Logs;

        ActionsRunner.Instance.LogsChanged += async () =>
        {
            OnPropertyChanged(nameof(Logs));
            await Task.Delay(50);
            if (Logs.Count > 0)
            {
                LogCollection.ScrollTo(Logs[^1], position: ScrollToPosition.End, animate: true);
            }
        };

        Expander.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(Expander.IsExpanded) && Expander.IsExpanded)
            {
                Debug.WriteLine("EXPANDED");
                // Give the layout time to expand fully
                await Task.Delay(150);

                if (Logs.Count > 0)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LogCollection.ScrollTo(Logs[^1], position: ScrollToPosition.End, animate: true);
                    });
                }
            }
        };
    }

    private ObservableCollection<string> _logs = [];
    public ObservableCollection<string> Logs
    {
        get => _logs;
        set
        {
            if (_logs != value)
            {
                _logs = value;
                OnPropertyChanged(nameof(Logs));
            }
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void CopyToClipboard(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string text)
        {
            Clipboard.SetTextAsync(text);
            _ = NotificationHelper.ShowSnackbar("Copied to clipboard!");
        }
    }
}
