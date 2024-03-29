using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.Model
{
    public interface IOCR : INotifyPropertyChanged //需要继承INotifyPropertyChanged，否则切换属性时无法通知
    {
        Guid Identify { get; set; }

        OCRType Type { get; set; }

        bool IsEnabled { get; set; }

        string Name { get; set; }

        string Url { get; set; }

        string AppID { get; set; }

        string AppKey { get; set; }

        Task<string> ExecuteAsync(byte[] bytes, CancellationToken token);

        IOCR Clone();
    }

    public class OCRCollection<T> : BindingList<T>
        where T : IOCR
    {
        public event Action<T>? OnActiveOCRChanged;

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);

            // 当项被添加或者属性改变时，检查 IsEnabled 属性
            if (e.ListChangedType == ListChangedType.ItemAdded || (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.Name == nameof(IOCR.IsEnabled)))
            {
                T changedItem = this[e.NewIndex];
                if (changedItem.IsEnabled)
                {
                    OnActiveOCRChanged?.Invoke(changedItem);
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

        public OCRCollection<T> DeepCopy()
        {
            var copiedList = new OCRCollection<T>();
            foreach (var item in this)
            {
                T newItem = (T)Activator.CreateInstance(item.GetType())!;
                var properties = typeof(T).GetProperties();
                foreach (var property in properties)
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(newItem, property.GetValue(item));
                    }
                }
                copiedList.Add(newItem);
            }
            return copiedList;
        }
    }
}
