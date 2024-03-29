using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Base;
using STranslate.ViewModels.Preference;
using STranslate.ViewModels.Preference.History;
using System.Windows;
using System.Windows.Controls;

namespace STranslate.ViewModels
{
    public partial class PreferenceViewModel : WindowVMBase
    {
        private PerferenceType _pType;

        //手动重写取消判断value是否更新，每次都触发避免手动点击切换导航（convertback-donothing）
        //再进入设置首页（convert）导航选中UI不更新的问题
        public PerferenceType PType
        {
            get => _pType;
            set
            {
                OnPropertyChanging();
                _pType = value;
                OnPropertyChanged();
            }
        }

        public void UpdateNavigation(PerferenceType type = PerferenceType.Common)
        {
            PType = type;

            switch (type)
            {
                case PerferenceType.Hotkey:
                    HotkeyPage();
                    break;

                case PerferenceType.Service:
                    ServicePage();
                    break;

                case PerferenceType.OCR:
                    OCRPage();
                    break;

                case PerferenceType.TTS:
                    TTSPage();
                    break;

                case PerferenceType.Favorite:
                case PerferenceType.History:
                    HistoryPage();
                    break;

                case PerferenceType.About:
                    AboutPage();
                    break;

                default:
                    CommonPage();
                    break;
            }
        }

        [RelayCommand]
        private void CommonPage() => CurrentView = Singleton<CommonViewModel>.Instance;

        [RelayCommand]
        private void HotkeyPage() => CurrentView = Singleton<HotkeyViewModel>.Instance;

        [RelayCommand]
        private void ServicePage() => CurrentView = Singleton<ServiceViewModel>.Instance;

        [RelayCommand]
        private void OCRPage() => CurrentView = Singleton<Preference.OCRViewModel>.Instance;

        [RelayCommand]
        private void TTSPage() => CurrentView = Singleton<TTSViewModel>.Instance;

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

        private readonly ScrollViewer view = new();

        public override void Close(Window win)
        {
            if (Singleton<HistoryViewModel>.Instance.HistoryDetailContent is UserControl view && view.DataContext is HistoryContentViewModel vm)
            {
                vm.TTSCancelCommand.Execute(null);
            }
            base.Close(win);
        }
    }
}
