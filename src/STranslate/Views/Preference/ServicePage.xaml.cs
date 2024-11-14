using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class ServicePage
{
    public ServicePage()
    {
        InitializeComponent();
        DataContext = Singleton<ServiceViewModel>.Instance;
    }
}