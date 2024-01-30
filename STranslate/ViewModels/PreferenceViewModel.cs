using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Util;
using STranslate.ViewModels.Base;
using STranslate.ViewModels.Preference;
using System.Threading.Tasks;

namespace STranslate.ViewModels
{
    public partial class PreferenceViewModel : WindowVMBase
    {
        public PreferenceViewModel()
        {
            // 初始化Page
            CommonPage();
        }

        [RelayCommand]
        private void CommonPage() => CurrentView = Singleton<CommonViewModel>.Instance;

        [RelayCommand]
        private void HotkeyPage() => CurrentView = Singleton<HotkeyViewModel>.Instance;

        [RelayCommand]
        private void ServicePage() => CurrentView = Singleton<ServiceViewModel>.Instance;

        [RelayCommand]
        private void FavoritePage() => CurrentView = Singleton<FavoriteViewModel>.Instance;

        [RelayCommand]
        private void HistoryPage()
        {
            CurrentView = Singleton<HistoryViewModel>.Instance;

            // 加载记录
            Singleton<HistoryViewModel>.Instance.LoadMoreHistoryCommand.Execute(view);
        }

        [RelayCommand]
        private void AboutPage() => CurrentView = Singleton<AboutViewModel>.Instance;

        [ObservableProperty]
        private object? _currentView;

        private readonly System.Windows.Controls.ScrollViewer view = new();
    }
}
