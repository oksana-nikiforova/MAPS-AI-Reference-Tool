using System.Diagnostics;

namespace MAPSAI.Views;

public partial class UploadElement : ContentView
{
	public UploadElement()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty UploadLabelProperty =
        BindableProperty.Create(
            nameof(UploadLabel),
            typeof(string),
            typeof(UploadElement),
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

    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
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
                PickerTitle = "Please select a file",
                FileTypes = FileType,
            };

            var fileResult = await FilePicker.PickAsync(options);

            if (fileResult is null) return;

            FilePath = fileResult.FullPath;
            IsFileChosen = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            IsFileChosen = false;
        }
    }

    private void DeleteFile(object sender, EventArgs e)
    {
        FilePath = string.Empty;
        IsFileChosen = false;
    }
}