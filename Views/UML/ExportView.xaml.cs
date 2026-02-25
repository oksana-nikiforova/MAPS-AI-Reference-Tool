using MAPSAI.Models;

namespace MAPSAI.Views.UML;

public enum ExportType
{
    Mermaid,
    PlantUML,
    Image
}

public class ExportRequest
{
    public ExportRequest(string diagramtype, ExportType exportType)
    {
        DiagramType = diagramtype;
        ExportType = exportType;
    }

    public string DiagramType { get; set; }

    public ExportType ExportType { get; set; }
}

public partial class ExportView : ContentView
{

    public static readonly BindableProperty RequestTypeProperty =
       BindableProperty.Create(
           nameof(RequestComponent),
           typeof(string),
           typeof(ExportView),
           string.Empty);

    public string RequestComponent
    {
        get => (string)GetValue(RequestTypeProperty);
        set => SetValue(RequestTypeProperty, value);
    }

    public static readonly BindableProperty ExportTitleProperty =
        BindableProperty.Create(
            nameof(ExportTitle),
            typeof(string),
            typeof(ExportView),
            string.Empty);

    public string ExportTitle
    {
        get => (string)GetValue(ExportTitleProperty);
        set => SetValue(ExportTitleProperty, value);
    }

    public ExportView()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void ExportDiagramText(object sender, EventArgs e)
    {
        var exportrequest = new ExportRequest(RequestComponent, ExportType.Mermaid);
        ActionsRunner.Instance.NotifyExportDiagramRequest(exportrequest);
    }

    private void ExportDiagramImage(object sender, EventArgs e)
    {
        var exportrequest = new ExportRequest(RequestComponent, ExportType.Image);
        ActionsRunner.Instance.NotifyExportDiagramRequest(exportrequest);
    }

    private void ExportDiagramTextPlantUML(object sender, EventArgs e)
    {
        var exportrequest = new ExportRequest(RequestComponent, ExportType.PlantUML);
        ActionsRunner.Instance.NotifyExportDiagramRequest(exportrequest);
    }
}