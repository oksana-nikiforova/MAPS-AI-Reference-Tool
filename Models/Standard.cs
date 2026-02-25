
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MAPSAI.Models
{
    public class Standard : INotifyPropertyChanged
    {
        public Standard(string name, string link, ObservableCollection<TreeNode<string>> tree)
        {
            ID = Guid.NewGuid().ToString();
            Name = name;
            Link = link;
            Tree = tree;
        }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<TreeNode<string>> Tree { get; set; } = [];

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
