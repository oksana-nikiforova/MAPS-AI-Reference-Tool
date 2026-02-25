using MAPSAI.Core.Models;
using MAPSAI.Models;
using MAPSAI.Services.Builders.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace MAPSAI.Services.Builders
{
    public class GanttBuilder : IBuilder
    {
        public const string DEFAULT_DIRECTION = "gantt";
        public const string DATE_FORMAT = "dateFormat YYYY-MM-DD";
        public const string Connector = ":";
        public const string AxisFormat = "axisFormat W";
        public const string WeekendExclude = "excludes weekends";

        public const string Styling =
            """
            %%{init: { 
                "theme": "base", 
                "themeVariables": { 
                    "primaryColor": "#003838", 
                    "primaryBorderColor": "#003838", 
                    "tertiaryColor": "#ffffff", 
                    "primaryTextColor": "#949494", 
                    "lineColor": "#d0d0d0", 
                    "background": "#ffffff" }, 
                "gantt": {
                    "barHeight": 28
                } 
                }}%%
            """;

        public GanttBuilder(){}

        public string Build(ObservableCollection<UserStory> stories)
        {
            if (stories.Count == 0)
            {
                return "";
            }

            var minStart = stories.Min(s => s.StartDate);
            var maxEnd = stories.Max(s => s.EndDate);

            int totaldays = (maxEnd - minStart).Days;

            var diagram = new StringBuilder();

            string tickInterval = "";

            switch (totaldays)
            {
                case (< 30):
                    tickInterval = "tickinterval 1day";
                    break;

                case (>= 30 and < 45):
                    tickInterval = "tickinterval 2day";
                    break;

                case (>= 45 and < 60):
                    tickInterval = "tickinterval 3day";
                    break;

                case (>= 60 and < 90):
                    tickInterval = "tickinterval 4day";
                    break;

                case (>= 90 and < 150):
                    tickInterval = "tickinterval 1week";
                    break;

                case (>= 170 and < 270):
                    tickInterval = "tickinterval 2week";
                    break;

                case (>= 270 and < 365):
                    tickInterval = "tickinterval 3week";
                    break;

                case (>= 365):
                    tickInterval = "tickinterval 1month";
                    break;
            }

            diagram.AppendLine(Styling);
            diagram.AppendLine(DEFAULT_DIRECTION);
            diagram.AppendLine(DATE_FORMAT);
            diagram.AppendLine(tickInterval);
            diagram.AppendLine(WeekendExclude);

            var index = 1;

            foreach (var story in stories)
            {
                if (story.Story.Contains("[external]") || story.User.Contains("[external]")) continue;

                string ganttTask = $"US{index:D3} {story.Story.Truncate(20)}/{story.User.Truncate(10)}/{Connector}{story.StartDate.ToString("yyyy-MM-dd")}, {story.EndDate.ToString("yyyy-MM-dd")}";

                diagram.AppendLine(ganttTask);

                index++;
            }

            Debug.WriteLine(diagram.ToString());
            return diagram.ToString();
        }
    }

    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            // Only substring and add dots if the text is actually longer than the limit
            return value.Length <= maxLength ? value : $"{value.Substring(0, maxLength)}...";
        }
    }
}
