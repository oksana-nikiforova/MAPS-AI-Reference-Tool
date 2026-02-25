using MAPSAI.Models;
using MAPSAI.Services.AI;
using MAPSAI.Services.Builders;

namespace MAPSAI.Views;

public partial class BindableEntryView : ContentView
{
    private ListEntryService _listEntryService;

    public BindableEntryView()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty EntryItemProperty =
        BindableProperty.Create(
            nameof(EntryItem),
            typeof(GeneratableEntry),
            typeof(BindableEntryView),
            default(GeneratableEntry));

    public GeneratableEntry EntryItem
    {
        get => (GeneratableEntry)GetValue(EntryItemProperty);
        set => SetValue(EntryItemProperty, value);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler?.MauiContext != null)
        {
            _listEntryService = Handler.MauiContext.Services.GetRequiredService<ListEntryService>();
        }
    }

    private async void Generate(object sender, EventArgs e)
    {
        _ = await EntryItem.GenerateAsync(_listEntryService);
    }
}