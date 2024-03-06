using STranslate.Util;
using STranslate.ViewModels.Preference;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace STranslate.Views.Preference
{
    /// <summary>
    /// ServicePage.xaml 的交互逻辑
    /// </summary>
    public partial class ServicePage : UserControl
    {
        public ServicePage()
        {
            InitializeComponent();
            DataContext = Singleton<ServiceViewModel>.Instance;
        }

        private bool isDragging = false;
        private object? itemBeingDragged = null;

        private void List_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListBoxItem)))
            {
                var droppedItem = (ListBoxItem)e.Data.GetData(typeof(ListBoxItem));
                var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

                if (droppedItem != null && targetItem != null)
                {
                    int droppedIndex = CurrentServiceListBox.Items.IndexOf(droppedItem.Content);
                    int targetIndex = CurrentServiceListBox.Items.IndexOf(targetItem.Content);

                    var itemToMove = Singleton<ServiceViewModel>.Instance.CurTransServiceList[droppedIndex];
                    Singleton<ServiceViewModel>.Instance.CurTransServiceList.RemoveAt(droppedIndex);
                    Singleton<ServiceViewModel>.Instance.CurTransServiceList.Insert(targetIndex, itemToMove);
                    Singleton<ServiceViewModel>.Instance.SelectedIndex = targetIndex;
                }
            }
        }

        private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 获取 ListBoxItem
            if (e.OriginalSource is DependencyObject originalSource)
            {
                ListBoxItem clickedItem = FindAncestor<ListBoxItem>(originalSource)!;

                // 如果点击的是 ListBoxItem 内的 ToggleButton，则不处理事件
                if (IsAncestorOfToggleButton(clickedItem, originalSource))
                {
                    e.Handled = false;
                    return;
                }
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                itemBeingDragged = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

                if (itemBeingDragged != null)
                {
                    isDragging = true;
                }
            }
        }

        private void List_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(CurrentServiceListBox, itemBeingDragged, DragDropEffects.Move);
                isDragging = false;
            }
        }

        private bool IsAncestorOfToggleButton(ListBoxItem item, DependencyObject target)
        {
            if (item == null)
            {
                return false;
            }

            ToggleButton? toggleButton = FindAncestor<ToggleButton>(target);

            return toggleButton != null && item.IsAncestorOf(toggleButton);
        }

        public static T? FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T t)
                {
                    return t;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        /// <summary>
        /// ListBox鼠标滚轮事件处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServiceListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //按住Ctrl滚动时不将滚动冒泡给上一层级的控件
            if (!e.Handled && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // ListBox拦截鼠标滚轮事件
                e.Handled = true;

                // 激发一个鼠标滚轮事件，冒泡给外层ListBox接收到
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = MouseWheelEvent, Source = sender };
                var parent = ((Control)sender).Parent as UIElement;
                parent!.RaiseEvent(eventArg);
            }
        }
    }
}
