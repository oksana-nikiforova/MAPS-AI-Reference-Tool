using MAPSAI.Core.Models;
using MAPSAI.Models;
using MAPSAI.Services.Builders.Interfaces;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace MAPSAI.Services.Builders
{
    public class UseCaseBuilder
    {
        public const string DEFAULT_DIRECTION = "flowchart LR";
        public const string ActorImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEIAAABuCAYAAACTOsWlAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAEQklEQVR42u2cz4tVZRjHP+PMeCssQSNQU0dqthZGQ6WbipLIwIUxbjRppbt0ZQshcKUbgxKrjS7d5o8I8g+oMFAoiiJikslBGDIT09C5Ld7vyzlOd5yZe855n3PPfT5wuJc5532e5/3cueec931nzgA2DAJPAGu0AUxquwbcM6orCY8C48Bp4AbQnmO7oWPG1aYxLAM+AG526PQ08L226Q77b6rtMutOFOUdYCrXscvAYWAMaHU4vqV9h3VsbDcF7LbuTDcMAx/nOnIReKWLOK+qbYzzETBk3bmF8hhwQYXfBt4FlhSIt0QxbivmBeWoNYPAORU8CTxfYuwx4A/FPqtcteWoCr0GjFQQf0Sx28AR687OxTYVeAfYXGGeLcC/yrXNutOzGQZ+UnH7E+Tbr1w/UrOT5z4V9huwNEG+lnK1gb3WnY8MABMqalfCvLuVc0I1mPMc2a1xq2CsxdAC/lbuTUWDFbm+R7br9UvCiTIVd5QzX4OpiNf0ej6hhEjM+XodRKzX688GImLOdQa572MYmCF8T0cM8m9Q7nsYX0afJBsQPWSQ/+Fc/jVFAhX9atzKvX/EQEQ+562uo5Qg4k/CqBBglYGImPMf4LqliDjKBFhtKGJStZiJgDDNBvCSgYg4uPvBIPf/2EM2C5Wa75R7j7UEgMcJl682aa/n68gunSutJUTOq6gTCXN+opznrDuf5xnCjdVdYDRBvlHlmgE2Wnd+NqcIn9BXVHuXN6QcbeCkdac7sZpsDeNYhXk+JFvrsLhkL4gXCcPjNnCAcidMBhQzLhO8YN3Z+dhFdv//KeVM3S0FPsvFTTkTVoi3Cff+beAbis1qb1aMtmLusO7cYtkEXCH7FM8ALxOG7vMxrGPP5NpfoYQpubmoetJzOXAQeI9smH6dMMV2CbiqDcK4YRXwLPCG2kI4HxwjLOj8VXG9lbMWOM79K+PzbVNqszZFgamnwQcJZ/u3gKcIt8lj2vct8DvwK2Ft82sa/pczeVaQ/QassCykjGF4I3ARwkUIFyFchHARwkUIFyFchHARwkUIFyGGCJMg64sG6pLlufcbsBt4TQwATwO/GBVQF0bj+kOhleSCxDkRyxrM8fmIuuEihIsQLkK4COEihIsQLkK4COEihIsQLkK4COEihIsQLkK4COEihIsQLkK4COEihIsQLkJYi3iT8ESyaWCrtQwrdpA9NCc+pGe7dVGWEs4CX/SjjLyEzwn/ndMie5BXX8joJCHSNzIeJKFvZCxEQuNlLEZCY2V0I6FxMopIaIyMMiT0vIwyJfSsjCok9JyMKiX0jIwUEmovI6WE2sqwkFA7GZYSaiOjDhLMZdRJgpmMOkpILqPOEpLJ6AUJlcvoJQmVyehFCaXL6GUJpclogoTCMpokoWsZTZSwaBlNlrBgGf0gYV4Z/SThgTIO9ZmE2TJmgPfjD3f2mYS8jHGA/wAmMI3jUyIYqAAAAABJRU5ErkJggg==";
        public const string noBoxClass = "classDef noBorder fill:none,stroke:none,color:black";
        public const string blackBoxClass = "classDef blackBox fill:none,stroke:black,color:black";
        public const string classImplementation = "class **current-class** noBorder";
        public const string boxClassImplementation = "class **current-class** blackBox";

        public UseCaseBuilder() {}

        public string Build(ObservableCollection<UserStory> stories, bool includeBase64 = false)
        {
            if (stories.Count == 0)
                return "";

            var diagram = new StringBuilder();

            diagram.AppendLine(DEFAULT_DIRECTION);
            diagram.AppendLine(noBoxClass);
            diagram.AppendLine(blackBoxClass);

            foreach (var story in stories)
            {
                if (story.Story.StartsWith("external") || story.User.StartsWith("external"))
                    continue;

                string userId = Regex.Replace(story.User.Trim(), @"[^\w]", "_");
                string storyId = Regex.Replace(story.Story.Trim(), @"[^\w]", "_");

                var tempString =
                    $"{userId}[<div style='width:50px; margin:auto;'><img src='{ActorImage}' style='height:80px;'/></div>{IBuilder.newLine}{story.User}]"
                    + IBuilder.Connector +
                    $"{storyId}([{Regex.Replace(Regex.Replace(story.Story.Trim(), @"[^\w']", " "), @"\\s+", " ")}])";

                var userClass = classImplementation.Replace("**current-class**", userId);
                var storyClass = boxClassImplementation.Replace("**current-class**", storyId);

                diagram.AppendLine(tempString);
                diagram.AppendLine(userClass);
                diagram.AppendLine(storyClass);
            }

            return diagram.ToString();
        }
    }
}
