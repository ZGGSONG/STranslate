﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class VocabularyBookPage : UserControl
{
    public VocabularyBookPage()
    {
        InitializeComponent();
        var vm = Singleton<VocabularyBookViewModel>.Instance;

        // 设置滚动到当前选中的服务
        vm.OnSelectedServiceChanged = () =>
            CurVocabularyBookListBox.ScrollIntoView(CurVocabularyBookListBox.SelectedItem);
        DataContext = vm;
    }

    public static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent(current);
        } while (current != null);

        return null;
    }
}