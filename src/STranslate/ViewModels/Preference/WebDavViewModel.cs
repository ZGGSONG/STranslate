using System.ComponentModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Base;
using WebDav;

namespace STranslate.ViewModels.Preference;

public partial class WebDavViewModel : WindowVMBase
{
    private string? _absolutePath;

    private string? _tmpPath;

    private WebDavClient? _webDavClient;

    [ObservableProperty] private BindingList<WebDavResult> _webDavResultList = [];

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

        var fullName = (string)objs.First();
        var view = (Window)objs.Last();

        var path = $"{_absolutePath.TrimEnd('/')}/{fullName}";
        using var response = await _webDavClient.GetRawFile(path);
        if (response.IsSuccessful && response.StatusCode == 200)
        {
            var zipFile = Path.Combine(_tmpPath, fullName);
            await using (var fileStream = File.Create(zipFile))
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
    private async Task Delete(string fullName)
    {
        if (_absolutePath == null || _webDavClient == null)
            return;

        var path = $"{_absolutePath.TrimEnd('/')}/{fullName}";
        var response = await _webDavClient.Delete(path);
        if (response.IsSuccessful && response.StatusCode == 204)
            WebDavResultList.Remove(Find(fullName));
        else
            ToastHelper.Show("删除失败", WindowType.Preference);
    }

    [RelayCommand]
    private void Update(string fullName)
    {
        Find(fullName).IsEdit = true;
    }

    [RelayCommand]
    private async Task ConfirmAsync(string fullName)
    {
        if (_absolutePath == null || _webDavClient == null || _tmpPath == null)
            return;

        //保存原始名
        var originFullName = fullName;
        var selectedValue = Find(fullName);
        var targetFullname = selectedValue.Name + ConstStr.ZIP;

        if (originFullName == targetFullname)
        {
            selectedValue.IsEdit = false;
            selectedValue.FullName = targetFullname;
            return;
        }

        //webdav operate
        var source = $"{_absolutePath.TrimEnd('/')}/{originFullName}";
        var target = $"{_absolutePath.TrimEnd('/')}/{targetFullname}";

        var response = await _webDavClient.Move(source, target);

        if (response.IsSuccessful && response.StatusCode == 201)
        {
            selectedValue.IsEdit = false;
            selectedValue.FullName = targetFullname;
        }
        else
        {
            ToastHelper.Show("修改名称失败", WindowType.Preference);
            LogService.Logger.Warn(
                $"WebDav 修改名称({originFullName}=>{selectedValue.FullName})失败, 原因描述: {response?.Description ?? "无返回描述"}");

            Cancel(originFullName);
        }
    }

    [RelayCommand]
    private void Cancel(string fullName)
    {
        var selectedValue = Find(fullName);
        selectedValue.Name = selectedValue.FullName.Replace(ConstStr.ZIP, string.Empty);
        selectedValue.IsEdit = false;
    }

    public override void Close(Window win)
    {
        win.DialogResult = false;
        base.Close(win);
    }

    private WebDavResult Find(string fullName)
    {
        return WebDavResultList.First(x => x.FullName == fullName);
    }
}