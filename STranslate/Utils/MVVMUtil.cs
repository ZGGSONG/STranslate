using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STranslate.Utils
{
    /// <summary>
    /// 通知
    /// </summary>
    public class BaseVM : INotifyPropertyChanged
    {
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
    }
    /// <summary>
    /// Command
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand(Predicate<object> canExecute, Action<object> execute)
        {
            this._canExecute = canExecute;
            this._execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    /// <summary>
    /// 单例辅助类
    /// </summary>
    public abstract class SingletonMode<T> where T : class
    {
        public static T _Instance;
        public static T Instance()
        {
            Type type = typeof(T);
            lock (type)
            {
                if (SingletonMode<T>._Instance == null)
                {
                    SingletonMode<T>._Instance = (Activator.CreateInstance(typeof(T), true) as T);
                }
                return SingletonMode<T>._Instance;
            }
        }
    }
}
