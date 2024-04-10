using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Base;
using WebDav;

namespace STranslate.ViewModels.Preference
{
    public partial class WebDavViewModel : WindowVMBase
    {
        [ObservableProperty]
        private BindingList<string> _webDavResultList = [];

        private WebDavClient? _webDavClient;

        private string? _absolutePath;

        private string? _tmpPath;

        public void UpdateParam(WebDavClient client, string absolutePath, string tmpPath)
        {
            _webDavClient = client;
            _absolutePath = absolutePath;
            _tmpPath = tmpPath;
        }

        [RelayCommand]
        private async Task Download(List<object> objs)
        {
            if (_absolutePath == null || _webDavClient == null || _tmpPath == null || objs.Count != 2)
                return;

            var name = (string)objs.First();
            var view = (Window)objs.Last();

            string path = $"{_absolutePath.TrimEnd('/')}/{name}";
            using var response = await _webDavClient.GetRawFile(path);
            if (response.IsSuccessful && response.StatusCode == 200)
            {
                var zipFile = Path.Combine(_tmpPath, name);
                using (var fileStream = File.Create(zipFile))
                {
                    await response.Stream.CopyToAsync(fileStream);
                }

                ZipUtil.DecompressToDirectory(zipFile, _tmpPath);
                view.DialogResult = true;
                base.Close(view);
            }
            else
            {
                ToastHelper.Show("删除失败", WindowType.Preference);
            }
        }

        [RelayCommand]
        private async Task Delete(string name)
        {
            if (_absolutePath == null || _webDavClient == null)
                return;

            string path = $"{_absolutePath.TrimEnd('/')}/{name}";
            var response = await _webDavClient.Delete(path);
            if (response.IsSuccessful && response.StatusCode == 204)
            {
                WebDavResultList.Remove(name);
            }
            else
            {
                ToastHelper.Show("删除失败", WindowType.Preference);
            }
        }

        public override void Close(Window win)
        {
            win.DialogResult = false;
            base.Close(win);
        }
    }
}
