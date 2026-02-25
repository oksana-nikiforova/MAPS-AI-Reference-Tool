using System.Globalization;
using System.Text.RegularExpressions;
using MAPSAI.Views.UML;


namespace MAPSAI.Converters
{
    public class Base64PlaceholderConverter : IValueConverter
    {
        // Regex for data:image/png;base64,....
        private static readonly Regex Base64Regex = new(
            @"data:image\/[a-zA-Z]+;base64,[A-Za-z0-9+/=]+",
            RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text)
                return string.Empty;

            // Replace embedded images with a placeholder
            return Base64Regex.Replace(text, "{{actor_icon}}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If needed, we can re-inject the Base64 image back before saving/exporting.
            if (value is not string text)
                return string.Empty;

            // If the placeholder is found, replace it with the actual Base64 data.
            if (text.Contains("{{actor_icon}}"))
            {
                // This is your embedded base64 constant from MermaidView
                return text.Replace("{{actor_icon}}", MermaidView.ActorImage);
            }

            return text;
        }
    }
}