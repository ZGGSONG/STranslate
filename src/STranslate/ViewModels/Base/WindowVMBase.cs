using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;

namespace STranslate.ViewModels.Base;

public partial class WindowVMBase : ObservableObject
{
    [ObservableProperty] private string _maximizeContent = Constant.MaximizeContent;

    [RelayCommand]
    private void Minimize(Window win)
    {
        win.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private void Maximize(Window win)
    {
        win.WindowState = win.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
    }

    [RelayCommand]
    public virtual void Close(Window win)
    {
        win.Close();
    }

    [RelayCommand]
    private void WindowStateChange(Window win)
    {
        MaximizeContent = win.WindowState switch
        {
            WindowState.Normal => Constant.MaximizeContent,
            WindowState.Maximized => Constant.MaximizeBackContent,
            _ => MaximizeContent
        };
    }
}