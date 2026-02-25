using MAPSAI.Core.Models;
using MAPSAI.Models;
using MAPSAI.Services.Builders.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace MAPSAI.Services.Builders
{
    public class PlantUMLGanttBuilder : IBuilder
    {
        public const string DIAGRAM_START = "@startgantt";
        public const string DIAGRAM_END = "@endgantt";

        public string Build(ObservableCollection<UserStory> stories)
        {
            var diagram = new StringBuilder();

            if (stories.Count == 0)
            {
                return "";
            }

            diagram.AppendLine(DIAGRAM_START);
            diagram.AppendLine($"Project starts {stories[0].StartDate.ToString("yyyy-MM-dd")}");

            foreach (var story in stories)
            {
                var ganttLine = $"[{story.Story}] starts {story.StartDate.ToString("yyyy-MM-dd")} and ends {story.EndDate.ToString("yyyy-MM-dd")}";
                diagram.AppendLine(ganttLine);
            }

            diagram.AppendLine(DIAGRAM_END);

            return diagram.ToString();
        }
    }
}
