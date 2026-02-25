using CommunityToolkit.Maui.Views;

public class GeneratedContentPopup : Popup
{
    public GeneratedContentPopup(string title, string? content)
    {
        var grid = new Grid
        {
            Padding = 16,
            RowSpacing = 8,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // title
                new RowDefinition(GridLength.Star), // scrollable content
                new RowDefinition(GridLength.Auto)  // button
            }
        };

        var titleLabel = new Label
        {
            Text = title,
            TextColor = Colors.Black,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold
        };
        Grid.SetRow(titleLabel, 0);

        var editor = new Editor
        {
            Text = content ?? "No content generated.",
            TextColor = Colors.Black,
            IsReadOnly = true,
            AutoSize = EditorAutoSizeOption.Disabled
        };

        var scroll = new ScrollView
        {
            Content = editor
        };
        Grid.SetRow(scroll, 1);

        var closeButton = new Button
        {
            Text = "Close",
            TextColor = Colors.Black,
            HorizontalOptions = LayoutOptions.End,
            Command = new Command(async () => await CloseAsync())
        };
        Grid.SetRow(closeButton, 2);

        grid.Children.Add(titleLabel);
        grid.Children.Add(scroll);
        grid.Children.Add(closeButton);

        Content = grid;
    }
}
