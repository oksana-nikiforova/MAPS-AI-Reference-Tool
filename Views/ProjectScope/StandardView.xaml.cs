using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui;
using MAPSAI.Models;
using MAPSAI.Services.Files;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MAPSAI.Views.Plan;


namespace MAPSAI.Views.ProjectScope;

public partial class StandardView : ContentView
{
    private ExcelService _excelService;
    private MSWordService _msWordService;

    private CancellationTokenSource? _generationCts;

    public StandardView()
    {
        InitializeComponent();
        BindingContext = this; 

        ActionsRunner.Instance.StandardParsingDone += OnTreeParsingDone;
        Standards = DataStore.Instance.Project.Standards;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler?.MauiContext != null && _excelService == null)
        {
            _excelService = Handler.MauiContext.Services.GetRequiredService<ExcelService>();
            _msWordService = Handler.MauiContext.Services.GetRequiredService<MSWordService>();
        }

        _ = LoadDefaultStandard();
    }

    private async Task<string> GetDefaultStandardPath()
    {
        const string fileName = "default_standard.xlsx";

        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);


        using var inputStream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using var outputStream = File.Create(localPath);
        await inputStream.CopyToAsync(outputStream);
        

        return localPath;
    }

    private async Task LoadDefaultStandard()
    {
        var defaultPlanPath = await GetDefaultStandardPath();
        var excelTree = _excelService.BuildTreeFromExcel(defaultPlanPath);

        const string defaultName = "Default standard";

        var existing = Standards.FirstOrDefault(s => s.Name == defaultName);
        if (existing != null)
        {
            Standards.Remove(existing);
        }

        Standard newStandard = new(defaultName, defaultPlanPath, excelTree.Children);
        Standards.Add(newStandard);
    }

    private string _planName = string.Empty;
    public string PlanName
    {
        get => _planName;
        set
        {
            _planName = value;
            OnPropertyChanged();
        }
    }

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


    private bool _isTreeParsing = false;
    public bool IsTreeParsing
    {
        get => _isTreeParsing;
        set
        {
            _isTreeParsing = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<Standard> _standards = [];

    public ObservableCollection<Standard> Standards
    {
        get => _standards;
        set
        {
            _standards = value;
            OnPropertyChanged();
        }
    }

    public Dictionary<string, string> PlanOutputDict = [];

    private Standard _activeStandard { get; set; }

    public Standard ActiveStandard
    {
        get => _activeStandard;
        set
        {
            _activeStandard = value;
            DataStore.Instance.ActiveStandard = value;
            OnPropertyChanged();
        }
    }

    public FilePickerFileType ExcelFileType { get; } = new(
    new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".xlsx" } },
    });

    private void SelectPlan(object sender, TappedEventArgs e)
    {
        if (IsTreeParsing)
        {
            return;
        }
        if (sender is Label label && label.BindingContext is Standard standard)
        {
            foreach (var item in Standards)
            {
                item.IsSelected = false;
            }

            standard.IsSelected = true;

            ActionsRunner.Instance.NotifyProjectPlanSelected(standard);
        }
    }

    private void DeletePlan(object sender, EventArgs e)
    {
        if (IsTreeParsing)
        {
            return;
        }

        if (sender is Button button && button.CommandParameter is Standard standard)
        {
            Standards.Remove(standard);
            DataStore.Instance.Project.Standards.Remove(standard);
            ActionsRunner.Instance.NotifyProjectPlanDeleted(standard);
        }
    }

    private void OnTreeParsingDone()
    {
        IsTreeParsing = false;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            _generationCts = new CancellationTokenSource();
            await GeneratePlan(_generationCts.Token);
        }
        catch (OperationCanceledException)
        {
            await Application.Current.MainPage.DisplayAlert("Canceled", "Generation was canceled.", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            IsTreeParsing = false;
        }
    }

    private void ClearNodeContent(TreeNode<string> node)
    {
        node.Content = string.Empty;

        foreach (var child in node.Children)
            ClearNodeContent(child);
    }

    public async Task<bool> GeneratePlan(CancellationToken token)
    {
        if (!HasAnyActiveNodes())
        {
            await Application.Current.MainPage.DisplayAlert(
                "No Active Nodes",
                "There are no active items in the tree to generate a plan for.",
                "OK"
            );
            return false;
        }

        IsTreeParsing = true; //vai atstaat?

        //PADOT ACTIVE STANDARD UZ POPUP

        var popup = new PlanProcessingPopup(ActiveStandard);

        await Application.Current.MainPage.ShowPopupAsync(popup, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        });

        //PlanOutputDict = outputDic;

        IsTreeParsing = false;
        return true;
    }



    private bool AnyActiveNode(TreeNode<string> node)
    {
        if (node.IsActive)
            return true;

        foreach (var child in node.Children)
        {
            if (AnyActiveNode(child))
                return true;
        }

        return false;
    }

    private bool HasAnyActiveNodes()
    {
        foreach (var root in ActiveStandard.Tree)
        {
            if (AnyActiveNode(root))
                return true;
        }

        return false;
    }

    private void CancelGeneration(object sender, EventArgs e)
    {
        _generationCts?.Cancel();
    }

    private void picker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (IsTreeParsing)
        {
            return;
        }
        if (ActiveStandard is not null)
        {
            foreach (var item in Standards)
            {
                item.IsSelected = false;
            }
            ActiveStandard.IsSelected = true;

            ActionsRunner.Instance.NotifyProjectPlanSelected(ActiveStandard);
        }
    }

}

