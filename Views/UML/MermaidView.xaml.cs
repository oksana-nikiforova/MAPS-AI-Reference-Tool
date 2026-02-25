using CommunityToolkit.Maui.Storage;
using MAPSAI.Models;
using MAPSAI.Services;
using MAPSAI.Services.Builders;
using System.Buffers.Text;
using System.Diagnostics;

namespace MAPSAI.Views.UML;

public partial class MermaidView : ContentView
{
    private PlantUMLBuilder _plantUMLBuilder;
    private PlantUMLGanttBuilder _plantUMLGanttBuilder;
    private PlantUMLProcessModelBuilder _plantUMLProcessModelBuilder;

    private UseCaseBuilder _useCaseBuilder;
    private GanttBuilder _ganttBuilder;
    private ProcessModelBuilder _processModelBuilder;

    public static readonly BindableProperty DiagramTypeProperty =
       BindableProperty.Create(
           nameof(DiagramType),
           typeof(string),
           typeof(MermaidView),
           string.Empty);

    public string DiagramType
    {
        get => (string)GetValue(DiagramTypeProperty);
        set => SetValue(DiagramTypeProperty, value);
    }

    public const string ActorImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEIAAABuCAYAAACTOsWlAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAEQklEQVR42u2cz4tVZRjHP+PMeCssQSNQU0dqthZGQ6WbipLIwIUxbjRppbt0ZQshcKUbgxKrjS7d5o8I8g+oMFAoiiJikslBGDIT09C5Ld7vyzlOd5yZe855n3PPfT5wuJc5532e5/3cueec931nzgA2DAJPAGu0AUxquwbcM6orCY8C48Bp4AbQnmO7oWPG1aYxLAM+AG526PQ08L226Q77b6rtMutOFOUdYCrXscvAYWAMaHU4vqV9h3VsbDcF7LbuTDcMAx/nOnIReKWLOK+qbYzzETBk3bmF8hhwQYXfBt4FlhSIt0QxbivmBeWoNYPAORU8CTxfYuwx4A/FPqtcteWoCr0GjFQQf0Sx28AR687OxTYVeAfYXGGeLcC/yrXNutOzGQZ+UnH7E+Tbr1w/UrOT5z4V9huwNEG+lnK1gb3WnY8MABMqalfCvLuVc0I1mPMc2a1xq2CsxdAC/lbuTUWDFbm+R7br9UvCiTIVd5QzX4OpiNf0ej6hhEjM+XodRKzX688GImLOdQa572MYmCF8T0cM8m9Q7nsYX0afJBsQPWSQ/+Fc/jVFAhX9atzKvX/EQEQ+562uo5Qg4k/CqBBglYGImPMf4LqliDjKBFhtKGJStZiJgDDNBvCSgYg4uPvBIPf/2EM2C5Wa75R7j7UEgMcJl682aa/n68gunSutJUTOq6gTCXN+opznrDuf5xnCjdVdYDRBvlHlmgE2Wnd+NqcIn9BXVHuXN6QcbeCkdac7sZpsDeNYhXk+JFvrsLhkL4gXCcPjNnCAcidMBhQzLhO8YN3Z+dhFdv//KeVM3S0FPsvFTTkTVoi3Cff+beAbis1qb1aMtmLusO7cYtkEXCH7FM8ALxOG7vMxrGPP5NpfoYQpubmoetJzOXAQeI9smH6dMMV2CbiqDcK4YRXwLPCG2kI4HxwjLOj8VXG9lbMWOM79K+PzbVNqszZFgamnwQcJZ/u3gKcIt8lj2vct8DvwK2Ft82sa/pczeVaQ/QassCykjGF4I3ARwkUIFyFchHARwkUIFyFchHARwkUIFyGGCJMg64sG6pLlufcbsBt4TQwATwO/GBVQF0bj+kOhleSCxDkRyxrM8fmIuuEihIsQLkK4COEihIsQLkK4COEihIsQLkK4COEihIsQLkK4COEihIsQLkK4COEihIsQLkJYi3iT8ESyaWCrtQwrdpA9NCc+pGe7dVGWEs4CX/SjjLyEzwn/ndMie5BXX8joJCHSNzIeJKFvZCxEQuNlLEZCY2V0I6FxMopIaIyMMiT0vIwyJfSsjCok9JyMKiX0jIwUEmovI6WE2sqwkFA7GZYSaiOjDhLMZdRJgpmMOkpILqPOEpLJ6AUJlcvoJQmVyehFCaXL6GUJpclogoTCMpokoWsZTZSwaBlNlrBgGf0gYV4Z/SThgTIO9ZmE2TJmgPfjD3f2mYS8jHGA/wAmMI3jUyIYqAAAAABJRU5ErkJggg==";

    public static readonly BindableProperty MermaidDiagramProperty =
        BindableProperty.Create(
            nameof(MermaidDiagram),
            typeof(string),
            typeof(MermaidView),
            string.Empty,
            propertyChanged: OnMermaidDiagramChanged);

    public string MermaidDiagram
    {
        get => (string)GetValue(MermaidDiagramProperty);
        set => SetValue(MermaidDiagramProperty, value);
    }

    public MermaidView()
	{
		InitializeComponent();
        BindingContext = this;

        ActionsRunner.Instance.ExportDiagramRequest += Instance_ExportDiagramText;
        ActionsRunner.Instance.UserStoriesChanged += Instance_UserStoriesChanged;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler?.MauiContext != null && _plantUMLBuilder == null)
        {
            _plantUMLBuilder = Handler.MauiContext.Services.GetRequiredService<PlantUMLBuilder>();
            _plantUMLGanttBuilder = Handler.MauiContext.Services.GetRequiredService<PlantUMLGanttBuilder>();
            _plantUMLProcessModelBuilder = Handler.MauiContext.Services.GetRequiredService<PlantUMLProcessModelBuilder>();
            _useCaseBuilder = Handler.MauiContext.Services.GetRequiredService<UseCaseBuilder>();
            _ganttBuilder = Handler.MauiContext.Services.GetRequiredService<GanttBuilder>();
            _processModelBuilder = Handler.MauiContext.Services.GetRequiredService<ProcessModelBuilder>();
        }


        if (MermaidWebView != null)
        {
            MermaidWebView.Navigated -= MermaidWebView_Navigated;
            MermaidWebView.Navigated += MermaidWebView_Navigated;
        }

        BuildDiagram();
    }

    private async void MermaidWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        // Now CoreWebView2 is guaranteed to exist
        // BUT Mermaid may still be rendering → wait for JS signal

        // Wait until Mermaid SVG exists
        await MermaidWebView.EvaluateJavaScriptAsync(@"
        new Promise(resolve => {
            const check = () => {
                if (document.querySelector('.mermaid svg')) resolve();
                else setTimeout(check, 50);
            };
            check();
        });
    ");

        var base64 = await CaptureDiagram();

        switch (DiagramType)
        {
            case "USE_CASE":
                DataStore.Instance.Project.userStoryDiagram = base64;
                break;
            case "GANTT":
                DataStore.Instance.Project.ganttDiagram = base64;
                break;
            case "PROCESS":
                DataStore.Instance.Project.processDiagram = base64;
                break;
        }
    }

    public async void BuildDiagram()
    {
        if (_useCaseBuilder == null) return;

        switch (DiagramType)
        {
            case "USE_CASE":
                var dia = _useCaseBuilder.Build(DataStore.Instance.Project.UserStories);

                if (MermaidDiagram == dia) return;

                MermaidDiagram = dia;
                break;

            case "GANTT":
                var dia1 = _ganttBuilder.Build(DataStore.Instance.Project.UserStories);

                if (MermaidDiagram == dia1) return;

                MermaidDiagram = dia1;
                break;

            case "PROCESS":
                var dia2 = _processModelBuilder.Build(DataStore.Instance.Project.UserStories);

                if (MermaidDiagram == dia2) return;

                MermaidDiagram = dia2;
                break;
        }
    }

    private void Instance_UserStoriesChanged()
    {
        LoadMermaidDiagram(MermaidDiagram);
    }

    private async void Instance_ExportDiagramText(ExportRequest export_request)
    {
        if (DiagramType != export_request.DiagramType)
        {
            return;
        }

        switch (export_request.ExportType)
        {
            case (ExportType.Mermaid): 
                switch (export_request.DiagramType)
                {
                    case "USE_CASE":
                        await SaveDiagramText(MermaidDiagram, "use_case", "mermaid");
                        break;

                    case "GANTT":
                        await SaveDiagramText(MermaidDiagram, "roadmap", "mermaid");
                        break;

                    case "PROCESS":
                        await SaveDiagramText(MermaidDiagram, "process_model", "mermaid");
                        break;
                }
                break;

            case (ExportType.PlantUML):
                switch (export_request.DiagramType)
                {
                    case "USE_CASE":
                        var diaUse = _plantUMLBuilder.Build(DataStore.Instance.Project.UserStories);
                        _ = SaveDiagramText(diaUse, "use_case", "plantuml");
                        break;

                    case "GANTT":
                        var diaGantt = _plantUMLGanttBuilder.Build(DataStore.Instance.Project.UserStories);
                        _ = SaveDiagramText(diaGantt, "roadmap", "plantuml");
                        break;

                    case "PROCESS":
                        var diaProcess = _plantUMLProcessModelBuilder.Build(DataStore.Instance.Project.UserStories);
                        _ = SaveDiagramText(diaProcess, "process_model", "plantuml");
                        break;
                }
                break;

            case (ExportType.Image):
                var diagramBase64String = await CaptureDiagram();
                switch (export_request.DiagramType)
                {
                    case "USE_CASE":
                        _ = SaveImageFromBase64Async(diagramBase64String, "use_case");
                        break;

                    case "GANTT":
                        _ = SaveImageFromBase64Async(diagramBase64String, "roadmap");
                        break;

                    case "PROCESS":
                        _ = SaveImageFromBase64Async(diagramBase64String, "process_model");
                        break;
                }
                
                break;
        }
    }

    private static void OnMermaidDiagramChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MermaidView view && newValue is string diagram)
        {
            view.LoadMermaidDiagram(diagram);
        }
    }

    public static async Task<string> GetMermaidScriptAsync()
    {
        const string fileName = "mermaid.min.js";

        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        if (!File.Exists(localPath))
        {
            using var inputStream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var outputStream = File.Create(localPath);
            await inputStream.CopyToAsync(outputStream);
        }

        return localPath;
    }



    public async void LoadMermaidDiagram(string mermaidText)
    {
        if (string.IsNullOrWhiteSpace(mermaidText) || DataStore.Instance.Project.UserStories.Count == 0)
        {
            MermaidWebView.IsVisible = false;
            FallbackImage.IsVisible = true;
            return;
        }

        MermaidWebView.IsVisible = true;
        FallbackImage.IsVisible = false;


        string html = $@"
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset='utf-8' />
        <meta name='viewport' content='width=device-width, initial-scale=1' />
        <script src='../mermaid.min.js'></script>
        <style>
            html, body {{
                background-color: white !important;
                color: black !important;
                margin: 0;
                padding: 20px;
                overflow: auto;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            }}
            .mermaid {{
                overflow: visible;
                text-align: center;
                background-color: white !important;
                visibility: hidden;
                opacity: 0;
                transition: opacity .2s ease-in;
            }}
            svg {{
                max-width: 100%;
                height: auto;
                background-color: white !important;
            }}
            </style>
            <script type=""text/javascript"">
                window.exportPngResult = null;
    
                async function exportPng() {{
                    try {{
                        const svg = document.querySelector('.mermaid svg');
                        if (!svg) {{
                            window.exportPngResult = null;
                            return null;
                        }}
            
                        const svgData = new XMLSerializer().serializeToString(svg);
                        const base64String = btoa(unescape(encodeURIComponent(svgData)));
            
                        return new Promise((resolve) => {{
                            const img = new Image();
                            img.onload = function () {{
                                const canvas = document.createElement('canvas');
                                canvas.width = img.naturalWidth * 6;
                                canvas.height = img.naturalHeight * 6;
                                const ctx = canvas.getContext('2d');
                                ctx.fillStyle = 'white';
                                ctx.fillRect(0, 0, canvas.width, canvas.height);
                                ctx.drawImage(img, 0, 0);
                                const result = canvas.toDataURL('image/png').split(',')[1];
                                window.exportPngResult = result;
                                resolve(result);
                            }};
                            img.onerror = function() {{
                                window.exportPngResult = null;
                                resolve(null);
                            }};
                            img.src = ""data:image/svg+xml;base64,"" + base64String;
                        }});
                    }} catch (error) {{
                        console.error('Export error:', error);
                        window.exportPngResult = null;
                        return null;
                    }}
                }}
    
                function getExportResult() {{
                    return window.exportPngResult;
                }}
            </script>
            </head>
            <body>
            <div class='mermaid'>
            {mermaidText}
            </div>
            <script>
                mermaid.initialize({{ startOnLoad: false }});

                document.addEventListener(""DOMContentLoaded"", async () => {{
                    await mermaid.run({{ querySelector: '.mermaid' }});
                    document.querySelector('.mermaid').style.visibility = ""visible"";
                    document.querySelector('.mermaid').style.opacity = '1';
                }});
            </script>
            </body>
            </html>";

        MermaidWebView.Source = new HtmlWebViewSource
        {
            Html = html
        };
    }

    public async Task<string> CaptureDiagram()
    {
        try
        {
            await MermaidWebView.EvaluateJavaScriptAsync("exportPng()");

            await Task.Delay(1000);

            var diagramBase64String = await MermaidWebView.EvaluateJavaScriptAsync("getExportResult()");

            return diagramBase64String;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error exporting image: {ex.Message}");
            return "";
        }
    }

    private async Task SaveDiagramText(string diagramCode, string type, string codeType, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(diagramCode))
        {
            Debug.WriteLine("Diagram code: no data to save.");
            return;
        }

        var fileName = $"{type}_diagram_code_{codeType}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(diagramCode));
        stream.Position = 0;

        var result = await FileSaver.Default.SaveAsync(fileName, stream, token);

        if (result.IsSuccessful)
        {
            Debug.WriteLine($"Diagram code saved to: {result.FilePath}");
            _ = NotificationHelper.ShowSnackbar("Diagram code save successfully!", "Ok");
        }
        else
        {
            Debug.WriteLine($"Diagram code save failed: {result.Exception?.Message}");
            _ = NotificationHelper.ShowSnackbar("Diagram code save failed!", "Ok");
        }
    }

    private async Task SaveImageFromBase64Async(string? base64String, string diaType, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(base64String))
        {
            Debug.WriteLine("SaveImageFromBase64Async: no data to save.");
            return;
        }

        var commaIdx = base64String.IndexOf(',');
        if (base64String.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIdx >= 0)
        {
            base64String = base64String[(commaIdx + 1)..];
        }

        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(base64String);
        }
        catch (FormatException fex)
        {
            Debug.WriteLine($"SaveImageFromBase64Async: invalid Base64. {fex}");
            return;
        }

        var fileName = $"{diaType}_diagram_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        using var stream = new MemoryStream(imageBytes);

        stream.Position = 0;

        var result = await FileSaver.Default.SaveAsync(fileName, stream, token);

        if (result.IsSuccessful)
        {
            Debug.WriteLine($"Image saved to: {result.FilePath}");
            _ = NotificationHelper.ShowSnackbar("Image save successfully!", "Ok");
        }
        else
        {
            Debug.WriteLine($"Image save failed: {result.Exception?.Message}");
            _ = NotificationHelper.ShowSnackbar("Image save failed!", "Ok");
        }
    }
}