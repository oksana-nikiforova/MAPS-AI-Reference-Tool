using System.ComponentModel;

namespace MAPSAI.Models
{
    public class DownloadState : INotifyPropertyChanged
    {
        private bool _downloading;
        public bool Downloading
        {
            get => _downloading;
            set
            {
                if (_downloading != value)
                {
                    _downloading = value;
                    OnPropertyChanged(nameof(Downloading));
                }
            }
        }

        private string _status = string.Empty;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        private double _percentDone;
        public double PercentDone
        {
            get => _percentDone;
            set
            {
                if (_percentDone != value && value is not double.NaN)
                {
                    _percentDone = value;
                    OnPropertyChanged(nameof(PercentDone));
                }
            }
        }

        private double _completed;
        public double Completed
        {
            get => _completed;
            set
            {
                if (_completed != value && value is not double.NaN)
                {
                    _completed = value;
                    OnPropertyChanged(nameof(Completed));
                }
            }
        }

        private double _total;
        public double Total
        {
            get => _total;
            set
            {
                if (_total != value && value is not double.NaN)
                {
                    _total = value;
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
