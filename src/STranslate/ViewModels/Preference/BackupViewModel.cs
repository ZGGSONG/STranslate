using CommunityToolkit.Mvvm.ComponentModel;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference
{
    public partial class BackupViewModel : ObservableObject
    {
        [ObservableProperty]
        private BackupType _backupType;
    }
}
