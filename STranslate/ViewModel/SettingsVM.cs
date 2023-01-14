using STranslate.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STranslate.ViewModel
{
    public class SettingsVM : BaseVM
    {
        public SettingsVM()
        {
            LoadedCmd = new RelayCommand((_) => true, (_) =>
              {
                  Console.WriteLine("123");
              });
            ClosedCmd = new RelayCommand((_) => true, (_) =>
              {
                  Console.WriteLine("123");
              });
        }
        public ICommand LoadedCmd { get; private set; }
        public ICommand ClosedCmd { get; private set; }
    }
}
