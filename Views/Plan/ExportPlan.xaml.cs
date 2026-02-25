using MAPSAI.Models;
using MAPSAI.Services.Files;
using System.Diagnostics;

namespace MAPSAI.Views.Plan;

public partial class ExportPlan : ContentView
{
    private ExcelService _excelService;
    private MSWordService _msWordService;

    public ExportPlan()
	{
		InitializeComponent();
        BindingContext = this;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler?.MauiContext != null && _excelService == null)
        {
            _excelService = Handler.MauiContext.Services.GetRequiredService<ExcelService>();
            _msWordService = Handler.MauiContext.Services.GetRequiredService<MSWordService>();
        }
    }

    private async void GenerateFile(object sender, EventArgs e)
    {
        if (DataStore.Instance.Project.PlanDictionary.Count == 0)
        {
            _ = Application.Current.MainPage.DisplayAlert("Error!", "Please generate sections to export!", "Ok");
            return;
        }

        if (sender is Button button && button.CommandParameter is string param)
        {

            foreach (var item in DataStore.Instance.Project.PlanTree)
            {
                Debug.WriteLine(item.Value);
                foreach (var root in DataStore.Instance.Project.PlanTree)
                {
                    DebugPrintTree(root);
                }
            }

            switch (param)
            {

                case "Word":
                    Debug.WriteLine("Exporting Word plan");
                    var res = await _msWordService.GenerateDocument(DataStore.Instance.Project.PlanTree, "Project plan", "IT_Plan");
                    break;

            }
        }

    }

    private static void DebugPrintTree(TreeNode<string> node, int indent = 0)
    {
        if (node == null) return;

        var pad = new string(' ', indent * 2);

        Debug.WriteLine($"{pad}- {node.Value} | Active={node.IsActive} | Level={node.Level} | Children={node.Children?.Count ?? 0} | ContentLen={(node.Content?.Length ?? 0)}");

        if (node.Children == null) return;

        foreach (var child in node.Children)
            DebugPrintTree(child, indent + 1);
    }
}