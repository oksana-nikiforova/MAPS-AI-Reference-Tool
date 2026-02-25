
using System.ComponentModel;

namespace MAPSAI.Models
{
    public class ViewChange : INotifyPropertyChanged
    {
        public ViewChange(string name, string diagramType)
        {
            Name = name;
            DiagramType = diagramType;
        }

        public string Id { get; set; } = "";

        public string Name { get; set; }

        public event Action Fired;

        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public string DiagramType { get; set; }

        public void Raise()
        {
            Fired?.Invoke();
            IsActive = true;
           
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
