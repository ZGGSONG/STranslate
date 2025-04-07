using STranslate.Helper;
using STranslate.ViewModels;
using System.Windows.Input;

namespace STranslate.Views;

public partial class SsTranslateView
{
    public SsTranslateView(SsTranslateViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        WindowHelper.HideFromAltTab(this);
        base.OnSourceInitialized(e);
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
