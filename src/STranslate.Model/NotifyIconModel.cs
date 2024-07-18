using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class NotifyIconModel : ObservableObject
{
    /// <summary>
    ///     托盘程序图标
    /// </summary>
    [ObservableProperty] private string _iconSource = ConstStr.ICON;

    /// <summary>
    ///     文字提示
    /// </summary>
    [ObservableProperty] private string _toolTip = string.Empty;
}