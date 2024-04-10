using CommunityToolkit.Mvvm.ComponentModel;
using STranslate.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDav;

namespace STranslate.ViewModels.Preference
{
    public partial class WebDavViewModel : WindowVMBase
    {
        [ObservableProperty]
        private BindingList<string> _webDavResultList = [];

        private WebDavClient? _webDavClient;

        private string? _absolutePath;

        public void UpdateParam(WebDavClient client, string absolutePath)
        {
            _webDavClient = client;
            _absolutePath = absolutePath;
        }
    }
}
