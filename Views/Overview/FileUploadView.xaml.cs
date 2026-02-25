using MAPSAI.Models;
using MAPSAI.Services;
using MAPSAI.Services.Builders;
using MAPSAI.Services.Files;

namespace MAPSAI.Views.ProjectDataFiles;

public partial class FileUploadView : ContentView
{
    private XmlService _xmlService;
    private FileTextReader _fileTextReader;
    private ExcelService _excelService;

    public FileUploadView()
	{
		InitializeComponent();
        BindingContext = this;
        ActionsRunner.Instance.SavedProjectLoaded += Instance_SaveLoaded;
	}

    private void Instance_SaveLoaded()
    {
        if (!string.IsNullOrWhiteSpace(DataStore.Instance.Project.BusinessProcessModelFile.Path))
        {
            XmlUpload.FilePath = DataStore.Instance.Project.BusinessProcessModelFile.Path;
            XmlUpload.IsFileChosen = true;
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler?.MauiContext != null && _xmlService == null)
        {
            _xmlService = Handler.MauiContext.Services.GetRequiredService<XmlService>();
            _fileTextReader = Handler.MauiContext.Services.GetRequiredService<FileTextReader>();
            _excelService = Handler.MauiContext.Services.GetRequiredService<ExcelService>();
        }
    }

    public FilePickerFileType XmlFileType { get; } = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
        { DevicePlatform.WinUI, new[] { ".xml" } },
        });

    private void Upload(object sender, EventArgs e)
    {

        if (!string.IsNullOrWhiteSpace(XmlUpload.FilePath))
        {
            DataStore.Instance.Project.BusinessProcessModelFile.Path = XmlUpload.FilePath;

            var res = _xmlService.ParseXml(XmlUpload.FilePath);

            if (!res.Success)
            {
                _ = Application.Current.MainPage.DisplayAlert("Error!", "Failed to parse draw.io .xml file!", "Ok");
            }
            else
            {
                DataStore.Instance.Project.UserStories.Clear();

                foreach (var story in res.UserStories)
                {
                    DataStore.Instance.Project.UserStories.Add(story);
                }
                ActionsRunner.Instance.NotifyUserStoriesChanged();
            }
        }

        ActionsRunner.Instance.NotifyFilesUpload();
        _ = NotificationHelper.ShowSnackbar("Files read", "Ok");
    }

}