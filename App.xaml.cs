using CommunityToolkit.Maui.Extensions;
using MAPSAI.Models;
using System.Diagnostics;
using MAPSAI.Views;
using MAPSAI.Core.Models;
using MAPSAI.Models.AI;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;
using Window = Microsoft.Maui.Controls.Window;
using CommunityToolkit.Maui.Storage;



#if WINDOWS
using Microsoft.UI.Windowing;
#endif

namespace MAPSAI
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public CustomWindow CustomWindow;

#if WINDOWS
        private Microsoft.UI.Xaml.Window? _mainNativeWindow;
        private bool _allowRealClose = false;
#endif

        public App(CustomWindow customWindow)
        {
            InitializeComponent();
            CustomWindow = customWindow;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            CustomWindow.Page = new AppShell();

#if WINDOWS
            CustomWindow.HandlerChanged += (_, _) =>
            {
                _mainNativeWindow = CustomWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

                if (_mainNativeWindow != null)
                {
                    _mainNativeWindow.AppWindow.Closing += OnAppWindowClosing;
                }
            };
#endif

            return CustomWindow;
        }

#if WINDOWS
        private async void OnAppWindowClosing(
            AppWindow sender,
            AppWindowClosingEventArgs args)
        {
            if (_mainNativeWindow == null ||
                sender != _mainNativeWindow.AppWindow)
            {
                return;
            }

            // If exit was explicitly requested, allow close
            if (_allowRealClose)
                return;

            // Cancel OS close
            args.Cancel = true;

            // Ask user on UI thread
            var result = await MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current.MainPage.DisplayAlert(
                    "Exit MAPS-AI",
                    "Do you want to save your work before exiting?",
                    "Save and Exit",
                    "Exit Without Saving"));

            // Cancel → do nothing, keep app running
            if (result == null)
                return;

            // Save + Exit
            if (result == true)
            {
                var title = string.IsNullOrWhiteSpace(DataStore.Instance.Project.Title)
                    ? "no_title"
                    : DataStore.Instance.Project.Title;

                title = title.Replace(" ", "_").ToLower();

                var result1 = await FileSaver.Default.SaveAsync(
                    $"project_{title}.json",
                    new MemoryStream());

                if (!result1.IsSuccessful)
                    return;

                try
                {
                    DataStore.Instance.Save(result1.FilePath);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Failed to save project:\n{ex.Message}",
                        "OK");
                }
            }

            _allowRealClose = true;

            _mainNativeWindow?.Close();
        }
#endif
    }
}