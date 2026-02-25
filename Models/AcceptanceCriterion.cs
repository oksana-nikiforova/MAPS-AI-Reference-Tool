using MAPSAI.Views.Global.Interfaces;
using System.ComponentModel;

namespace MAPSAI.Models
{
    public class AcceptanceCriterion : INotifyPropertyChanged, IListElement
    {
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        private bool _isUsed = false;
        public bool IsUsed
        {
            get => _isUsed;
            set
            {
                if (_isUsed != value)
                {
                    _isUsed = value;
                    OnPropertyChanged(nameof(IsUsed));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
