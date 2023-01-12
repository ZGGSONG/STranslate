using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    public class MainViewModel : INotifyPropertyChanged
    {

        private double _processValue;
        public double ProcessValue { get => _processValue; set => UpdateProperty(ref _processValue, value); }

        private string _version;
        public string Version { get => _version; set => UpdateProperty(ref _version, value); }

        #region notifychanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void UpdateProperty<T>(ref T properValue, T newValue, [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return;
            properValue = newValue;
            NotifyPropertyChanged(properName);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
