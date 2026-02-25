using CommunityToolkit.Maui.Alerts;
using System.Diagnostics;

namespace MAPSAI.Services
{
    public class NotificationHelper
    {
        public static async Task ShowSnackbar(string message, string actionText = "OK", int durationSeconds = 4)
        {
            try
            {
                var snackbar = Snackbar.Make(
                    message: message,
                    actionButtonText: actionText,
                    duration: TimeSpan.FromSeconds(durationSeconds)
                );

                await snackbar.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
