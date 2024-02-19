using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.Model
{
    public interface ITTS : INotifyPropertyChanged//需要继承INotifyPropertyChanged，否则切换属性时无法通知
    {
        Guid Identify { get; set; }

        TTSType Type { get; set; }

        bool IsEnabled { get; set; }

        string Name { get; set; }

        string Url { get; set; }

        string AppID { get; set; }

        string AppKey { get; set; }

        Task SpeakTextAsync(string text, CancellationToken token);
    }

    public class TTSCollection<T> : BindingList<T> where T : ITTS
    {
        public event Action<T>? OnActiveTTSChanged;

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);

            // 当项被添加或者属性改变时，检查 IsEnabled 属性
            if (e.ListChangedType == ListChangedType.ItemAdded || (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.Name == nameof(ITTS.IsEnabled)))
            {
                T changedItem = this[e.NewIndex];
                if (changedItem.IsEnabled)
                {
                    OnActiveTTSChanged?.Invoke(changedItem);
                    // 设置其他所有项的 IsEnabled 为 false
                    foreach (T item in this)
                    {
                        if (!ReferenceEquals(item, changedItem) && item.IsEnabled)
                        {
                            item.IsEnabled = false;
                        }
                    }
                }
            }
        }
    }
}
