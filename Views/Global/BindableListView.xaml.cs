using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using UraniumUI.Icons.FontAwesome;
using MAPSAI.Platforms.Windows;
using MAPSAI.Services;
using System.Windows.Input;
using MAPSAI.Views.Global.Interfaces;
using MAPSAI.Services.Builders;
using MAPSAI.Services.AI;
namespace MAPSAI.Views;

public partial class BindableListView : ContentView
{
    private INotifyCollectionChanged _observableItemsSource;
    private ListEntryService _listEntryService;

    public BindableListView()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler?.MauiContext != null )
        {
            _listEntryService = Handler.MauiContext.Services.GetRequiredService<ListEntryService>();
        }
    }

    private bool _isGenerating = false;
    public bool IsGenerating
    {
        get => _isGenerating;
        set
        {
            if (_isGenerating != value)
            {
                _isGenerating = value;
                OnPropertyChanged(nameof(IsGenerating));
            }
        }
    }

    private bool _isUsable = true;
    public bool IsUsable
    {
        get => _isUsable;
        set
        {
            if (_isUsable != value)
            {
                _isUsable = value;
                OnPropertyChanged(nameof(IsUsable));
            }
        }
    }

    public static readonly BindableProperty ItemSourceObjectProperty =
    BindableProperty.Create(
        nameof(ItemSourceObject),
        typeof(object),
        typeof(BindableListView),
        propertyChanged: OnUserStoryChanged);

    public object ItemSourceObject
    {
        get => GetValue(ItemSourceObjectProperty);
        set => SetValue(ItemSourceObjectProperty, value);
    }

    public static readonly BindableProperty ItemsSourceProperty =
    BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(BindableListView),
        propertyChanged: OnItemsSourceChanged);

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty DisplayPropertyProperty =
        BindableProperty.Create(
            nameof(DisplayProperty),
            typeof(string),
            typeof(BindableListView),
            default(string));

    public string DisplayProperty
    {
        get => (string)GetValue(DisplayPropertyProperty);
        set => SetValue(DisplayPropertyProperty, value);
    }

    public static readonly BindableProperty CollectionPropertyProperty =
        BindableProperty.Create(
            nameof(CollectionProperty),
            typeof(string),
            typeof(BindableListView),
            default(string));

    public string CollectionProperty
    {
        get => (string)GetValue(CollectionPropertyProperty);
        set => SetValue(CollectionPropertyProperty, value);
    }

    public static readonly BindableProperty CheckboxPropertyProperty =
        BindableProperty.Create(
            nameof(CheckboxProperty),
            typeof(string),
            typeof(BindableListView),
            default(string));

    public string CheckboxProperty
    {
        get => (string)GetValue(CheckboxPropertyProperty);
        set => SetValue(CheckboxPropertyProperty, value);
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BindableListView view)
        {
            if (view._observableItemsSource != null)
            {
                view._observableItemsSource.CollectionChanged -= view.OnCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged observable)
            {
                view._observableItemsSource = observable;
                view._observableItemsSource.CollectionChanged += view.OnCollectionChanged;
            }

            view.BuildForm();
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        BuildForm();
    }

    private static void OnUserStoryChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BindableListView view)
        {
            view.ResolveItemsSource(newValue);
        }
    }

    private void BuildForm()
    {
        FormContainer.Children.Clear();
        if (ItemsSource == null || string.IsNullOrEmpty(DisplayProperty))
            return;

        foreach (var item in ItemsSource)
        {
            var propertyInfo = item.GetType().GetProperty(DisplayProperty, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
                continue;

            var row = new Grid
            {
                ColumnSpacing = 10,
                BindingContext = item
            };

            row.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = Microsoft.Maui.GridLength.Star },
                new ColumnDefinition { Width = Microsoft.Maui.GridLength.Auto },
                new ColumnDefinition { Width = Microsoft.Maui.GridLength.Auto }
            };

            var editor = new Editor
            {
                Placeholder = DisplayProperty,
                AutoSize = EditorAutoSizeOption.TextChanges, 
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                //TextWrapping = TextWrapping.Wrap, 
                MinimumHeightRequest = 60,
            };

            Grid.SetColumn(editor, 0);
            editor.SetBinding(Editor.TextProperty, DisplayProperty);
            row.Children.Add(editor);

            if (!string.IsNullOrEmpty(CheckboxProperty))
            {
                var boolPropertyInfo = item.GetType().GetProperty(CheckboxProperty, BindingFlags.Public | BindingFlags.Instance);
                if (boolPropertyInfo != null && boolPropertyInfo.PropertyType == typeof(bool))
                {
                    var checkBox = new CheckBox();
                    Grid.SetColumn(checkBox, 1);
                    checkBox.SetBinding(CheckBox.IsCheckedProperty, CheckboxProperty);
                    row.Children.Add(checkBox);
                }
            }

            var deleteButton = new Button
            {
                WidthRequest = 80,
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Colors.Transparent,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FASolid",
                    Glyph = Solid.Trash,
                    Color = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Primary"],
                    Size = 20
                },
            };
            Grid.SetColumn(deleteButton, 2);
            deleteButton.Clicked += (s, e) => OnDeleteItem(item);
            CursorBehavior.SetCursor(deleteButton, CursorIcon.Hand);

            row.Children.Add(deleteButton);
            FormContainer.Children.Add(row);
        }
    }

    private void ResolveItemsSource(object obj)
    {
        if (obj is null) return;

        var property = obj.GetType().GetProperty(CollectionProperty);
        if (property != null)
        {
            var collection = property.GetValue(obj) as IEnumerable;
            if (collection != null)
            {
                ItemsSource = collection;
            }
        }
    }

    private void OnDeleteItem(object item)
    {
        if (IsGenerating) return;

        if (ItemsSource is IList list && list.Contains(item))
        {
            list.Remove(item);
        }
    }

    private void OnAddClicked(object sender, EventArgs e)
    {
        if (IsGenerating) return;

        if (ItemsSource is IList list)
        {
            var itemType = list.GetType().GetGenericArguments().FirstOrDefault();
            if (itemType != null)
            {
                var newItem = Activator.CreateInstance(itemType);
                list.Add(newItem);
            }
        }
    }

    private void OnGenerateClicked(object sender, EventArgs e)
    {
        if (ItemSourceObject is null || IsGenerating) return;
        if (GenerateCommand?.CanExecute(ItemsSource) == true)
        {
            GenerateCommand.Execute(ItemsSource);
        }
    }

    public ICommand GenerateCommand => new Command(async () =>
    {
        IsGenerating = true;
        IsUsable = false;

        await Task.Delay(30);

        if (ItemsSource == null) return;

        if (ItemSourceObject is IGeneratableListFunction generatable)
        {
            var generatedItems = await generatable.GenerateAsync(_listEntryService);
            ItemsSource = generatedItems;
        }

        IsGenerating = false;
        IsUsable = true;
    });
}