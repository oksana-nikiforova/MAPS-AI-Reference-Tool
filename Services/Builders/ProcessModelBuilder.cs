using MAPSAI.Core.Models;
using MAPSAI.Models;
using MAPSAI.Services.Builders.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MAPSAI.Services.Builders
{
    public class ProcessModelBuilder: IBuilder
    {
        public const string DEFAULT_DIRECTION = "flowchart TD";
        public const string Seperator = "-------------";
        public const string blackBoxClass = "classDef blackBox fill:none,stroke:black,color:black";
        public const string boxClassImplementation = "class **current-class** blackBox";

        public ProcessModelBuilder() { }

        public string Build(ObservableCollection<UserStory> stories)
        {
            var diagram = new StringBuilder();

            diagram.AppendLine(DEFAULT_DIRECTION);
            diagram.AppendLine(blackBoxClass);

            var addedNodes = new HashSet<string>();

            foreach (var story in stories)
            {
                Debug.WriteLine(story.Story);
                if (story.Story.Contains("[external]") || story.User.Contains("[external]"))
                    continue;

                string storyId = story.ID.ToString();
                story.Story = StripHtml(story.Story);


                if (!addedNodes.Contains(storyId))
                {
                    diagram.AppendLine(
                        $"{storyId}([{Regex.Replace(Regex.Replace(story.Story.Trim(), @"[^\w']", " "), @"\s+", " ")} {IBuilder.newLine} {Seperator} {IBuilder.newLine} {story.User}])"
                    );
                    diagram.AppendLine(boxClassImplementation.Replace("**current-class**", storyId));
                    addedNodes.Add(storyId);
                }

                foreach (var connection in story.Connections)
                {
                    Debug.WriteLine("CONNECTIONS:");
                    Debug.WriteLine(connection);
                    var connectedNode = stories.FirstOrDefault(s => s.ID == connection.Target);
                    if (connectedNode is null) continue;

                    string connection_storyId = connectedNode.ID.ToString();

                    string arrow_text = "";
                    if (!string.IsNullOrWhiteSpace(connection.Text))
                    {
                        arrow_text = $"|{connection.Text}|";
                    }

                    story.Story = StripHtml(story.Story);



                    if (!addedNodes.Contains(connection_storyId))
                    {
                        diagram.AppendLine(
                            $"{connection_storyId}([{Regex.Replace(Regex.Replace(connectedNode.Story.Trim(), @"[^\w']", " "), @"\s+", " ")} {IBuilder.newLine} {Seperator} {IBuilder.newLine} {connectedNode.User}])"
                        );
                        diagram.AppendLine(boxClassImplementation.Replace("**current-class**", connection_storyId));
                        addedNodes.Add(connection_storyId);
                    }

                    var arrow = $"{storyId} {IBuilder.Connector}{arrow_text}{connection_storyId}";

                    if (!addedNodes.Contains(arrow))
                    {
                        diagram.AppendLine(arrow);
                        addedNodes.Add(arrow);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return diagram.ToString();
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Decode HTML entities first
            input = System.Net.WebUtility.HtmlDecode(input);

            // Remove tags
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }

}

