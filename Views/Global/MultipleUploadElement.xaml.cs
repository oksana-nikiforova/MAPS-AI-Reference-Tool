using MAPSAI.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MAPSAI.Views;

public partial class MultipleUploadElement : ContentView
{
    public MultipleUploadElement()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty UploadLabelProperty =
    BindableProperty.Create(
        nameof(UploadLabel),
        typeof(string),
        typeof(MultipleUploadElement),
        default(string));

    public string UploadLabel
    {
        get => (string)GetValue(UploadLabelProperty);
        set => SetValue(UploadLabelProperty, value);
    }

    private bool _isFileChosen;
    public bool IsFileChosen
    {
        get => _isFileChosen;
        set
        {
            _isFileChosen = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<Models.FileInfo> _fileList = [];
    public ObservableCollection<Models.FileInfo> Filelist
    {
        get => _fileList;
        set
        {
            _fileList = value;
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty FileTypeProperty =
    BindableProperty.Create(
        nameof(FileType),
        typeof(FilePickerFileType),
        typeof(UploadElement),
        default(FilePickerFileType));

    public FilePickerFileType FileType
    {
        get => (FilePickerFileType)GetValue(FileTypeProperty);
        set => SetValue(FileTypeProperty, value);
    }

    private async void OnFilePickerClicked(object sender, EventArgs e)
    {
        try
        {
            PickOptions options = new()
            {
                PickerTitle = "Please select files",
                FileTypes = FileType
            };

            var fileResult = await FilePicker.PickMultipleAsync(options);

            if (fileResult is null) return;

            if(Filelist.Count < 5)
            {
                var currentCount = Filelist.Count;
                var max = 5;
                fileResult = fileResult.Take(max - currentCount).ToList();

                foreach (var file in fileResult)
                {
                    var newFile = new Models.FileInfo() { Path = file.FullPath };

                    Filelist.Add(newFile);
                }
            }
            
            IsFileChosen = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            IsFileChosen = false;
        }
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        Filelist.Clear();

        DataStore.Instance.Project.Files.Clear();
    }
}