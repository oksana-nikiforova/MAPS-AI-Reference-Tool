
using MAPSAI.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MAPSAI.Services;
using MAPSAI.Platforms.Windows;
using UraniumUI.Icons.FontAwesome;
using MAPSAI.Pages;
using CommunityToolkit.Maui.Extensions;
namespace MAPSAI.Views.Plan;

public partial class PlanTreeView : ContentView
{
    public PlanTreeView()
    {
        InitializeComponent();
        BindingContext = this;

        ActionsRunner.Instance.ProjectPlanSelected += LoadNodes;
        ActionsRunner.Instance.ProjectPlanDeleted += DeleteStandard;
    }

    private ObservableCollection<TreeNode<string>> _nodes = new();

    private string _activeStandardID { get; set; } = string.Empty;

    public ObservableCollection<TreeNode<string>> RootNodes
    {
        get => _nodes;
        set
        {
            _nodes = value;
            OnPropertyChanged();
        }
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            OnPropertyChanged();
        }
    }

    private bool _isShowed;
    public bool IsShowed
    {
        get => _isShowed;
        set
        {
            _isShowed = value;
            OnPropertyChanged();
        }
    }

    private async Task<View> CreateNodeView(TreeNode<string> node, int level)
    {
        await Task.Delay(1);

        var stack = new VerticalStackLayout
        {
            Padding = new Thickness(level * 20, 0, 0, 0),
            Spacing = 2
        };

        var container = new HorizontalStackLayout
        {
            Spacing = 5
        };

        var button = new Button
        {
            Text = node.Value,
            CommandParameter = node,
            BackgroundColor = Colors.Transparent,
            CornerRadius = 0,
            HorizontalOptions = LayoutOptions.Start,
        };

        button.Clicked += OpenGeneratedContent;

        var expandButton = new Button
        {
            WidthRequest = 80,
            HorizontalOptions = LayoutOptions.End,
            BackgroundColor = Colors.Transparent,
            ImageSource = new FontImageSource
            {
                FontFamily = "FASolid",
                Glyph = Solid.ArrowRight,
                Color = (Color)Application.Current.Resources["Primary"],
                Size = 20
            },
            CommandParameter = node
        };

        var checkBox = new CheckBox
        {
            BindingContext = node
        };

        checkBox.SetBinding(CheckBox.IsCheckedProperty, nameof(node.IsActive), BindingMode.TwoWay);
        checkBox.CheckedChanged += CheckBox_PropertyChanged;

        expandButton.Clicked += ExpandNode;

        CursorBehavior.SetCursor(expandButton, CursorIcon.Hand);

        var label = new Label()
        {
            Text = $"({node.ChildrenCount})",
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions= LayoutOptions.Center,
        };

        if (node.ChildrenCount > 0)
        {
            container.Children.Add(expandButton);
        }
        else
        {
            container.Children.Add(new BoxView
            {
                WidthRequest = 80,
                HeightRequest = 20,
                Opacity = 0 // invisible but takes space
            });
        }

        container.Children.Add(checkBox);
        container.Children.Add(button);
        container.Children.Add(label);

        stack.Children.Add(container);

        var childrenStack = new VerticalStackLayout
        {
            IsVisible = node.IsExpanded
        };

        node.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(node.IsExpanded))
            {
                childrenStack.IsVisible = node.IsExpanded;

                if (node.IsExpanded)
                    expandButton.ImageSource = new FontImageSource
                    {
                        FontFamily = "FASolid",
                        Glyph = Solid.ArrowDown,
                        Color = (Color)Application.Current.Resources["Primary"],
                        Size = 20
                    };
                else
                    expandButton.ImageSource = new FontImageSource
                    {
                        FontFamily = "FASolid",
                        Glyph = Solid.ArrowRight,
                        Color = (Color)Application.Current.Resources["Primary"],
                        Size = 20
                    };
            }
        };

        foreach (var child in node.Children)
        {
            var childView = await CreateNodeView(child, level + 1);
            childrenStack.Children.Add(childView);
        }

        stack.Children.Add(childrenStack);

        return stack;
    }

    private void CheckBox_PropertyChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is TreeNode<string> node)
        {
            bool isChecked = e.Value;

            if (!isChecked)
            {
                UncheckDescendants(node);
                return;
            }

            // NEW: check all descendants
            CheckDescendants(node);

            var parent = node.Parent;
            while (parent != null)
            {
                if (!parent.IsActive)
                {
                    parent.IsActive = true;
                }

                parent = parent.Parent;
            }
        }
    }

    private void CheckDescendants(TreeNode<string> node)
    {
        foreach (var child in node.Children)
        {
            if (!child.IsActive)
                child.IsActive = true;

            CheckDescendants(child);
        }
    }

    private void UncheckDescendants(TreeNode<string> node)
    {
        foreach (var child in node.Children)
        {
            if (child.IsActive) child.IsActive = false;
            UncheckDescendants(child);
        }
    }

    public void SelectAll(object sender, EventArgs e)
    {
        if (RootNodes.Count == 0)
            return;

        bool setActive = !RootNodes[0].IsActive;

        foreach (var node in RootNodes)
        {
            SetNodeActiveRecursive(node, setActive);
        }
    }

    private void SetNodeActiveRecursive(TreeNode<string> node, bool isActive)
    {
        node.IsActive = isActive;

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                SetNodeActiveRecursive(child, isActive);
            }
        }
    }

    private async void LoadNodes(object? sender, Standard standard)
    {
        if (_activeStandardID == standard.ID)
        {
            ActionsRunner.Instance.NotifyStandardParsingDone();
            return;
        }
        else
        {
            _activeStandardID = standard.ID;
        }

        IsRunning = true;
        MainStackLayout.Children.Clear();
        RootNodes.Clear();

        var horStack = new HorizontalStackLayout()
        {
            Spacing = 10
        };

        var selectAllButton = new Button()
        {
            Text = "Select All",
            BackgroundColor = (Color)Application.Current.Resources["Primary"],
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Start,
            CornerRadius = 6
        };

        var activityIndicator = new ActivityIndicator()
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsRunning");
        activityIndicator.SetBinding(ActivityIndicator.IsVisibleProperty, "IsRunning");

        CursorBehavior.SetCursor(selectAllButton, CursorIcon.Hand);

        selectAllButton.Clicked += SelectAll;

        horStack.Children.Add(selectAllButton);
        horStack.Children.Add(activityIndicator);

        MainStackLayout.Children.Add(horStack);

        foreach (var item in standard.Tree)
        {
            RootNodes.Add(item);
        }

        foreach (var node in RootNodes)
        {
            var nodeView = await CreateNodeView(node, 0);
            MainStackLayout.Children.Add(nodeView);
        }

        IsRunning = false;

        ActionsRunner.Instance.NotifyStandardParsingDone();
    }

    public void ExpandNode(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is TreeNode<string> node)
        {
            node.IsExpanded = !node.IsExpanded;
            Debug.WriteLine($"Toggled '{node.Value}' to {node.IsExpanded}");
        }
    }

    public void OpenGeneratedContent(object sender, EventArgs e)
    {
        if (sender is Button button &&
            button.CommandParameter is TreeNode<string> node)
        {
            var popup = new GeneratedContentPopup(node.Value, node.Content);

            Application.Current?.MainPage?.ShowPopup(popup);
        }
    }

    private void DeleteStandard(object? sender, Standard standard)
    {
        if (standard.ID == _activeStandardID)
        {
            MainStackLayout.Children.Clear();
            RootNodes.Clear();
            _activeStandardID = string.Empty;
        }
    }
}