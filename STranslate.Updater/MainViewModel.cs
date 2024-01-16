using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace STranslate.Updater
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private double _processValue;

        public double ProcessValue
        {
            get => _processValue;
            set => UpdateProperty(ref _processValue, value);
        }

        private string _version = "";

        public string Version
        {
            get => _version;
            set => UpdateProperty(ref _version, value);
        }

        public MainViewModel() { }

        public MainViewModel(string version)
        {
            Version = version;
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void UpdateProperty<T>(ref T properValue, T newValue, [CallerMemberName] string properName = "")
        {
            if (Equals(properValue, newValue))
                return;
            properValue = newValue;
            NotifyPropertyChanged(properName);
        }

        public void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion PropertyChanged
    }
}
