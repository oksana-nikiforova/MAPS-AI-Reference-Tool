using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DocumentFormat.OpenXml.Wordprocessing;
using MAPSAI.Core.Models;
using MAPSAI.Models;
using MAPSAI.Models.AI;
using MAPSAI.Services.AI;
using MAPSAI.Services.Builders;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MAPSAI.Views.Plan;

public class AgentRunRequest
{
    public string SectionExact { get; set; } = "";
    public string PromptBody { get; set; } = "";
    public int Level { get; set; } = 1;
    public string VectorContext { get; set; }
    public Dictionary<string, string>? ToolResults { get; set; }
}

public class AgentRunResponse
{
    public string SectionExact { get; set; } = "";
    public string Content { get; set; } = "";
}


public partial class PlanProcessingPopup : Popup
{

    private CancellationTokenSource _cts = new();

    public PlanProcessingPopup(Standard activeStandard)
	{
        InitializeComponent();
        Loaded += OnLoaded;
        ActiveStandard = activeStandard;
    }

    private Standard ActiveStandard;

    private int _generationProgress = 0;
    public int GenerationProgress
    {
        get => _generationProgress;
        set
        {
            _generationProgress = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Progress));
        }
    }

    private int _generationTotal = 0;
    public int GenerationTotal
    {
        get => _generationTotal;
        set
        {
            _generationTotal = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Progress));
        }
    }

    public double Progress => GenerationTotal > 0 ? (double)GenerationProgress / GenerationTotal : 0.0;

    private void OnCancelClicked(object sender, EventArgs e)
    {
        // Disable cancel button immediately
        ((Button)sender).IsEnabled = false;

        // Switch UI into "cancelling" mode
        InfoLabel.Text = "Cancelling, please wait...";
        CancelIndicator.IsVisible = true;
        CancelIndicator.IsRunning = true;

        // Cancel the token
        _cts.Cancel();
    }

    private async void OnLoaded(object sender, EventArgs e)
    {
        await Task.Delay(300);
        await GeneratePlanSections(_cts.Token);
    }

    private async Task GeneratePlanSections(CancellationToken token)
    {
        try
        {
            GenerationProgress = 0;
            GenerationTotal = 0;

            foreach (var root in ActiveStandard.Tree)
            {
                GenerationTotal += CountActiveNodes(root);
            }

            foreach (var root in ActiveStandard.Tree)
            {
                token.ThrowIfCancellationRequested();
                var process = await TraverseAndGenerate(root, token);

                if (!process)
                {
                    _ = Application.Current.MainPage.DisplayAlert("Error!", "AI request failed!", "Ok");
                    //return false;
                }
            }

            token.ThrowIfCancellationRequested();

            var outputDic = new Dictionary<string, string>();
            foreach (var root in ActiveStandard.Tree)
            {
                await CollectActiveNodes(root, outputDic, token);
            }

            DataStore.Instance.Project.PlanDictionary = outputDic;

            DataStore.Instance.Project.PlanTree = ActiveStandard.Tree;

            InfoLabel.Text = "All selected plan section processed successfully!";

            await Task.Delay(1500);

            await Shell.Current.ClosePopupAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = "Failed to generate!";
            Debug.WriteLine(ex.ToString());
            await Shell.Current.ClosePopupAsync();
        }
    }

    private int CountActiveNodes(TreeNode<string> node)
    {
        if (node == null) return 0;
        node.Content = "";
        var count = node.IsActive ? 1 : 0;
        foreach (var child in node.Children)
            count += CountActiveNodes(child);
        return count;
    }

    private async Task<bool> TraverseAndGenerate(TreeNode<string> node, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        foreach (var child in node.Children)
        {
            await TraverseAndGenerate(child, token);
        }

        if (node.IsActive)
        {
            token.ThrowIfCancellationRequested();

            var level = GetNodeLevel(node);
            node.Level = level;

            Debug.WriteLine($"{node.Value} IS LEVEL {level}");

            ProgressLabel.Text = $"Generating {node.Value}...";

            AiCallResponse response = new();

            var sb = new StringBuilder();

            string childrenSummary = "";

            if (level < 2)
            {
                childrenSummary = string.Join("\n\n", node.Children.Select(child =>
                    $"Section Title: {child.Value}\nSection Content:\n{child.Content.Trim()}"
                ));
            }

            var apiRes = await SendAgentRequestAsync(node.Value, level, childrenSummary);

            Debug.WriteLine($"-------------GOT API RESPONSE for {node.Value}-------------");
            Debug.WriteLine(apiRes);

            node.Content = apiRes;

            OutputLabel.Text = "Generating...";
            GenerationProgress++;
        }

        return true;
    }

    private int GetNodeLevel(TreeNode<string> node)
    {
        int level = 1;
        var current = node.Parent;
        while (current != null)
        {
            level++;
            current = current.Parent;
        }
        return level - 1;
    }

    private async Task<bool> CollectActiveNodes(TreeNode<string> node, Dictionary<string, string> collector, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (node.IsActive)
            collector[node.Value] = node.Content;

        foreach (var child in node.Children)
            await CollectActiveNodes(child, collector, token);

        return true;
    }


    public static async Task<string> SendAgentRequestAsync(string section_name, int level, string childrenSummary = "")
    {
        var project = DataStore.Instance.Project;

        var requestBody = new AgentRunRequest
        {
            SectionExact = section_name,
            PromptBody = "", // intentionally empty
            Level = level,
            VectorContext = childrenSummary,
            ToolResults = new Dictionary<string, string>
            {
                ["get_user_stories"] = FormatUserStories(project.UserStories),

                ["get_project_purpose"] = project.ProjectPurpose?.Text ?? "",

                ["get_project_background"] = project.Background?.Text ?? "",

                ["get_project_deliverables"] = FormatListItemCollection(project.Deliverables?.Collection),

                ["get_project_resource_requirements"] = FormatListItemCollection(project.ResourceRequirements?.Collection),

                ["get_project_operations_and_support"] = project.OperationsSupport?.Text ?? "",

                ["get_project_safety_and_security_risks"] = FormatListItemCollection(project.SafetySecurity?.Collection),

                ["get_project_stakeholders"] = FormatListItemCollection(project.Stakeholders?.Collection),
            }
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        using var client = new HttpClient();
        //client.BaseAddress = new Uri("https://maps-ai-service-api.onrender.com");
        client.BaseAddress = new Uri("http://localhost:3000");

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/agent/run", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {response.StatusCode}\n{error}");
        }

        return await response.Content.ReadAsStringAsync();
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

