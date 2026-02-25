using MAPSAI.Core.Models;
using MAPSAI.Models;
using MAPSAI.Services.Builders.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MAPSAI.Services.Builders
{
    public class PlantUMLProcessModelBuilder : IBuilder
    {
        public const string DIAGRAM_START = "@startuml";
        public const string DIAGRAM_END = "@enduml";
        public const string newline = "%n()";
        public const string Seperator = "-------------";

        public string Build(ObservableCollection<UserStory> stories)
        {
            var diagram = new StringBuilder();

            diagram.AppendLine(DIAGRAM_START);

            foreach (var story in stories)
            {
                if (story.Story.Contains("[external]") || story.User.Contains("[external]")) continue;

                string storyId = Regex.Replace(story.Story.Trim(), @"[^\w]", "_");

                foreach (var connection in story.Connections)
                {
                    var connectedNode = stories.FirstOrDefault(s => s.ID == connection.Target);

                    if (connectedNode is null) continue;

                    string connection_storyId = Regex.Replace(connectedNode.Story.Trim(), @"[^\w]", "_");

                    string arrow_text = "";

                    if (!string.IsNullOrWhiteSpace(connection.Text))
                    {
                        arrow_text = $"|{connection.Text}|";
                    }

                    var tempString = $"rectangle \"{story.Story} {newline} {Seperator} {newline} {story.User}\" as {storyId}";
                    var connectionString = $"rectangle \"{connectedNode.Story} {newline} {Seperator} {newline} {connectedNode.User}\" as {connection_storyId}";

                    diagram.AppendLine(tempString);
                    diagram.AppendLine(connectionString);

                    var con1 = $"{storyId} --> {connection_storyId} : \"{connection.Text}\"";

                    diagram.AppendLine(con1);
                }
            }

            diagram.AppendLine(DIAGRAM_END);

            return diagram.ToString();
        }
    }
}
