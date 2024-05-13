using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Controls;
using STranslate.Util;

namespace STranslate.ViewModels.Preference
{
    public partial class AboutViewModel : ObservableObject
    {
        [ObservableProperty]
        private string version = "";

        public AboutViewModel()
        {
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? ConstStr.DEFAULTVERSION;
        }

        [RelayCommand]
        private void OpenLink(string url) => Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });

        [RelayCommand(IncludeCancelCommand = true)]
        private async Task CheckUpdateAsync(CancellationToken token)
        {
            try
            {
                var resp = await HttpUtil.GetAsync(ConstStr.GITHUBRELEASEURL, token);
                var parseData = JsonConvert.DeserializeObject<JObject>(resp);
                var remoteVer = parseData?["tag_name"]?.ToString() ?? ConstStr.DEFAULTVERSION;
                var canUpdate = StringUtil.IsCanUpdate(remoteVer, Version);
                MessageBox_S.Show(canUpdate ? $"检测到最新版本: {remoteVer}\n当前版本: {Version}" : $"恭喜您, 当前为最新版本!");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox_S.Show($"检查更新出错, 请检查网络情况");
                LogService.Logger.Warn($"检查更新出错, 请检查网络情况, {ex.Message}");
            }
        }
    }
}
