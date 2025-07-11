﻿using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Base;
using STranslate.ViewModels.Preference;
using STranslate.ViewModels.Preference.History;

namespace STranslate.ViewModels;

public partial class PreferenceViewModel : WindowVMBase
{
    private readonly ScrollViewer _view = new();

    [ObservableProperty] private object? _currentView;

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

            case PerferenceType.Translator:
                ServicePage();
                Singleton<ServiceViewModel>.Instance.SelectedIndex = 0;
                break;

            case PerferenceType.OCR:
                ServicePage();
                Singleton<ServiceViewModel>.Instance.SelectedIndex = 1;
                break;

            case PerferenceType.TTS:
                ServicePage();
                Singleton<ServiceViewModel>.Instance.SelectedIndex = 2;
                break;

            case PerferenceType.VocabularyBook:
                VocabularyBookPage();
                Singleton<ServiceViewModel>.Instance.SelectedIndex = 3;
                break;

            case PerferenceType.Replace:
                ReplacePage();
                break;

            case PerferenceType.Favorite:
            case PerferenceType.History:
                HistoryPage();
                break;

            case PerferenceType.Backup:
                BackupPage();
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
    private void CommonPage()
    {
        CurrentView = Singleton<CommonViewModel>.Instance;
    }

    [RelayCommand]
    private void HotkeyPage()
    {
        CurrentView = Singleton<HotkeyViewModel>.Instance;
    }

    [RelayCommand]
    private void TranslatorPage()
    {
        CurrentView = Singleton<TranslatorViewModel>.Instance;
    }

    [RelayCommand]
    private void ReplacePage()
    {
        CurrentView = Singleton<ReplaceViewModel>.Instance;
    }

    [RelayCommand]
    private void OCRPage()
    {
        CurrentView = Singleton<OCRScvViewModel>.Instance;
    }

    [RelayCommand]
    private void TTSPage()
    {
        CurrentView = Singleton<TTSViewModel>.Instance;
    }

    [RelayCommand]
    private void FavoritePage()
    {
        CurrentView = Singleton<FavoriteViewModel>.Instance;
    }

    [RelayCommand]
    private void HistoryPage()
    {
        CurrentView = Singleton<HistoryViewModel>.Instance;

        // 加载记录
        Singleton<HistoryViewModel>.Instance.LoadMoreHistoryCommand.Execute(_view);
    }

    [RelayCommand]
    private void BackupPage()
    {
        CurrentView = Singleton<BackupViewModel>.Instance;
    }

    [RelayCommand]
    private void AboutPage()
    {
        CurrentView = Singleton<AboutViewModel>.Instance;
        Singleton<AboutViewModel>.Instance.CheckLogCommand.Execute(null);
    }

    [RelayCommand]
    private void ServicePage()
    {
        // 更新侧边栏Check属性
        PType = PerferenceType.Service;
        CurrentView = Singleton<ServiceViewModel>.Instance;
    }

    [RelayCommand]
    private void VocabularyBookPage()
    {
        CurrentView = Singleton<VocabularyBookViewModel>.Instance;
    }

    public override void Close(Window win)
    {
        if (Singleton<HistoryViewModel>.Instance.HistoryDetailContent is UserControl { DataContext: HistoryContentViewModel vm }) vm.TTSCancelCommand.Execute(null);
        Singleton<AboutViewModel>.Instance.CheckUpdateCancelCommand.Execute(null);
        base.Close(win);
    }

    [RelayCommand]
    private void Save()
    {
        switch (CurrentView)
        {
            case CommonViewModel:
                Singleton<CommonViewModel>.Instance.SaveCommand.Execute(null);
                break;

            case HotkeyViewModel:
                Singleton<HotkeyViewModel>.Instance.SaveCommand.Execute(null);
                break;

            //case TranslatorViewModel:
            //    Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
            //    break;

            //case OCRScvViewModel:
            //    Singleton<OCRScvViewModel>.Instance.SaveCommand.Execute(null);
            //    break;

            //case TTSViewModel:
            //    Singleton<TTSViewModel>.Instance.SaveCommand.Execute(null);
            //    break;

            //case VocabularyBookViewModel:
            //    Singleton<VocabularyBookViewModel>.Instance.SaveCommand.Execute(null);
            //    break;

            case ReplaceViewModel:
                Singleton<ReplaceViewModel>.Instance.SaveCommand.Execute(null);
                break;

            case BackupViewModel:
                Singleton<BackupViewModel>.Instance.SaveCommand.Execute(null);
                break;

            case ServiceViewModel vm:
                {
                    //分页面保存
                    switch (vm.SelectedIndex)
                    {
                        case 0:
                            Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
                            break;
                        case 1:
                            Singleton<OCRScvViewModel>.Instance.SaveCommand.Execute(null);
                            break;
                        case 2:
                            Singleton<TTSViewModel>.Instance.SaveCommand.Execute(null);
                            break;
                        case 3:
                            Singleton<VocabularyBookViewModel>.Instance.SaveCommand.Execute(null);
                            break;
                        default:
                            break;
                    }
                }
                break;
        }
    }
}