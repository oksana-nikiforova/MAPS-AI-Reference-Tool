using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MAPSAI.Models
{
    public class TreeNode<T> : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isActive;

        public T Value { get; set; }

        public string Instruction { get; set; }

        private ObservableCollection<TreeNode<T>> _children = new();

        public ObservableCollection<TreeNode<T>> Children
        {
            get => _children ??= new();
            set => _children = value ?? new();
        }

        public int ChildrenCount => Children.Count;

        public int Level = 0;

        public TreeNode<T> Parent { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        private string _content;
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        private double? _totalDuration { get; set; } = 0;

        public double? TotalDuration
        {
            get => _totalDuration;
            set
            {
                _totalDuration = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public TreeNode(T value, string instruction = "")
        {
            Value = value;
            Instruction = instruction;
        }

        public void AddChild(TreeNode<T> child, string ?content = null)
        {
            child.Parent = this;

            if (!string.IsNullOrWhiteSpace(content))
            {
                child.Content = content;
            }
            
            Children.Add(child);
            Debug.WriteLine($"Added Parent to child! parent value: {this.Value}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
