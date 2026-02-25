using MAPSAI.Core.Models;
using MAPSAI.Models;
using MAPSAI.Services.Builders.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace MAPSAI.Services.Builders
{
    public class PlantUMLBuilder : IBuilder
    {

        public const string DIAGRAM_START = "@startuml";
        public const string DIAGRAM_END = "@enduml";
        public const string DIAGRAM_DIRECTION = "left to right direction";
        public const string Connector = "-->";
        
        public string Build(ObservableCollection<UserStory> stories)
        {
            var tempDict = new Dictionary<string, ObservableCollection<UserStory>>();

            var diagram = new StringBuilder();

            diagram.AppendLine(DIAGRAM_START);
            diagram.AppendLine(DIAGRAM_DIRECTION);

            foreach (var story in stories)
            {
                if (tempDict.ContainsKey(story.User))
                {
                    tempDict[story.User].Add(story);
                }
                else
                {
                    tempDict[story.User] = [story];
                }
            }

            var i = 0;
            foreach (var kvp in tempDict)
            {
                string actorLine = $"actor \"{kvp.Key}\"";
                diagram.AppendLine(actorLine);

                foreach (var story in kvp.Value)
                {
                    string useCaseLine = $"usecase \"{story.Story}\" as UC{i}";
                    diagram.AppendLine(useCaseLine);

                    string connection = $"\"{kvp.Key}\" {Connector} UC{i}";
                    diagram.AppendLine(connection);
                    i++;
                }
            }

            diagram.AppendLine(DIAGRAM_END);

            return diagram.ToString();
        }
    }
}
