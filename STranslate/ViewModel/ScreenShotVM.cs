using STranslate.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STranslate.ViewModel
{
    public class ScreenShotVM : BaseVM
    {
        public ScreenShotVM(Window ui)
        {
            _ScreenShotWin = ui;
            EscCmd = new RelayCommand((_) => true, (_) =>
            {
                _ScreenShotWin.Close();
            });


        }

        public ICommand EscCmd { get; private set; }
        public ICommand MouseMoveCmd { get; private set; }
        public ICommand MouseLeftDownCmd { get; private set; }
        public ICommand MouseLeftUpCmd { get; private set; }

        private Window _ScreenShotWin;
    }
}
