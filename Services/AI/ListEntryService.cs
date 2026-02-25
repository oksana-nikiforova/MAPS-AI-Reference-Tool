using MAPSAI.Core.Models;
using MAPSAI.Models;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MAPSAI.Services.AI
{
    public class ListEntryService
    {

        public async Task<string> SendPromptRequestAsync(string promptKey, string ?artefactName = null, string ?instruction = null)
        {
            var project = DataStore.Instance.Project;

            // Map prompt variables depending on promptKey
            var vars = new Dictionary<string, string>();

            switch (promptKey)
            {
                case "AcceptanceCriteriaPrompt":
                    vars["MEDIUM_CURRENT_STORY_TO_PROCESS"] = BuildCurrentStoryMediumString(DataStore.Instance.SelectedStory) ?? "";
                    break;

                case "PurposePrompt":
                    vars["CURRENT_STORY_TO_PROCESS"] = BuildCurrentStoryString(DataStore.Instance.SelectedStory) ?? "";
                    break;

                case "PriorityPrompt":
                    vars["CURRENT_STORY_TO_PROCESS"] = BuildCurrentStoryString(DataStore.Instance.SelectedStory) ?? "";
                    vars["HIGH_LEVEL_USER_STORIES"] = FormatUserStories(project.UserStories) ?? "";
                    break;

                case "DefaultEntryPrompt":
                    vars["PROJECT_TITLE"] = project.Title ?? "";
                    vars["HIGH_LEVEL_USER_STORIES"] = FormatUserStories(project.UserStories);
                    vars["ARTEFACT_NAME"] = artefactName ?? "";
                    vars["ARTEFACT_INSTRUCTION"] = instruction ?? "";
                    break;

                case "DefaultListPrompt":
                case "DeliverablesPrompt":
                case "ResourceReqPrompt":
                case "SafetySecurityPrompt":
                case "StakeholdersPrompt":
                    vars["PURPOSE_OF_PROJECT"] = project.ProjectPurpose?.Text ?? "";
                    vars["HIGH_LEVEL_USER_STORIES"] = FormatUserStories(project.UserStories);
                    vars["ARTEFACT_NAME"] = artefactName ?? "";
                    vars["ARTEFACT_INSTRUCTION"] = instruction ?? "";
                    break;

                default:
                    throw new Exception($"Unsupported promptKey: {promptKey}");
            }

            var requestBody = new
            {
                promptKey = promptKey,
                vars = vars
                // optional:
                // systemMessage = "You are a formal project management assistant."
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://maps-ai-service-api.onrender.com")
            };

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/agent/run-prompt", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Request failed: {response.StatusCode}\n{error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            // Deserialize only the AI response field
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("aiResponse", out var aiResponseElement))
            {
                return aiResponseElement.GetString() ?? "";
            }

            return responseJson; // fallback
        }

        public static string BuildCurrentStoryMediumString(UserStory currentstory)
        {
            if (currentstory == null) return "";

            var userStoriesBuilder = new StringBuilder();
            userStoriesBuilder.AppendLine($"As {currentstory.User} I want to {currentstory.Story} so that {currentstory.Purpose.Text}.");

            return userStoriesBuilder.ToString();
        }

        public static string BuildCurrentStoryString(UserStory currentstory)
        {
            if (currentstory == null) return "";

            var userStoriesBuilder = new StringBuilder();
            userStoriesBuilder.AppendLine($"As {currentstory.User} I want to {currentstory.Story} so that {currentstory.Purpose.Text}.");
            userStoriesBuilder.AppendLine("Acceptance critera:");
            foreach (var item in currentstory.AcceptanceCriteria.Collection)
            {
                userStoriesBuilder.AppendLine(item.Text);
            }
            userStoriesBuilder.AppendLine($"Priority: {currentstory.Priority}");
            userStoriesBuilder.AppendLine();

            return userStoriesBuilder.ToString();
        }

        private static string FormatUserStories(IEnumerable<UserStory>? stories)
        {
            if (stories == null) return "";

            var sb = new StringBuilder();
            foreach (var s in stories)
            {
                // Build a stable story line.
                // NOTE: Purpose is GeneratableEntry; in your code you display "Text" elsewhere, but here you used Purpose as object.
                // We'll try Purpose.Text if it exists; otherwise fall back to ToString().
                var purposeText = GetStringPropertyOrToString(s.Purpose, "Text");

                sb.Append("- As ").Append(s.User?.Trim() ?? "a user")
                  .Append(", I want to ").Append(s.Story?.Trim() ?? "")
                  .Append("."); // keep it clean

                if (!string.IsNullOrWhiteSpace(purposeText))
                    sb.Append(" So that ").Append(purposeText.Trim().TrimEnd('.')).Append('.');

                if (!string.IsNullOrWhiteSpace(s.Priority))
                    sb.Append(" Priority: ").Append(s.Priority.Trim()).Append('.');

                if (!string.IsNullOrWhiteSpace(s.StoryPoints))
                    sb.Append(" Story points: ").Append(s.StoryPoints.Trim()).Append('.');

                // Optional: acceptance criteria (as nested bullets)
                var ac = s.AcceptanceCriteria?.Collection;
                if (ac != null && ac.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("  - Acceptance criteria:");
                    foreach (var item in ac)
                    {
                        var line = FormatListElement(item);
                        if (!string.IsNullOrWhiteSpace(line))
                            sb.Append("    - ").AppendLine(line);
                    }
                }

                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private static string FormatListItemCollection<T>(ObservableCollection<T>? items)
        {
            if (items == null || items.Count == 0) return "";

            var sb = new StringBuilder();
            foreach (var item in items)
            {
                var line = FormatListElement(item);
                if (!string.IsNullOrWhiteSpace(line))
                    sb.Append("- ").AppendLine(line);
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Tries common property names like Text/Name/Title/Description. Falls back to ToString().
        /// This keeps your MAUI types flexible without needing per-type serializers.
        /// </summary>
        private static string FormatListElement(object? item)
        {
            if (item == null) return "";

            // Try common semantic fields
            var s =
                GetStringProperty(item, "Text") ??
                GetStringProperty(item, "Name") ??
                GetStringProperty(item, "Title") ??
                GetStringProperty(item, "Description");

            if (!string.IsNullOrWhiteSpace(s))
                return s.Trim();

            // Fall back
            return item.ToString()?.Trim() ?? "";
        }

        private static string GetStringPropertyOrToString(object? obj, string propertyName)
        {
            if (obj == null) return "";
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(string))
                return (string?)prop.GetValue(obj) ?? "";
            return obj.ToString() ?? "";
        }

        private static string? GetStringProperty(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || prop.PropertyType != typeof(string)) return null;
            return (string?)prop.GetValue(obj);
        }

    }
}
