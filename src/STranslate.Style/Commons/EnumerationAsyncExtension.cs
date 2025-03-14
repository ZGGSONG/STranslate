using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Markup;

namespace STranslate.Style.Commons;

public class EnumerationAsyncExtension : MarkupExtension
{
    /// <summary>
    ///     每批加载的数量
    /// </summary>
    private readonly int _batchSize = 1000;

    public Type EnumType { get; set; }

    public EnumerationAsyncExtension(Type? enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an Enum.");
        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var collection = new AsyncObservableCollection<EnumerationMember>();
        LoadDataAsync(collection);
        return collection;
    }

    private async void LoadDataAsync(AsyncObservableCollection<EnumerationMember> collection)
    {
        await Task.Run(async () =>
        {
            var enumValues = Enum.GetValues(EnumType).Cast<object>();
            if (Enum.GetUnderlyingType(EnumType) == typeof(int))
            {
                enumValues = enumValues.OrderBy(e => (int)e);
            }

            var allItems = enumValues.Select(enumValue =>
                new EnumerationMember
                {
                    Value = enumValue,
                    Description = GetDescription(enumValue)
                }).ToList();

            for (int i = 0; i < allItems.Count; i += _batchSize)
            {
                var batch = allItems.Skip(i).Take(_batchSize);
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in batch)
                    {
                        collection.Add(item);
                    }
                });
                // 给UI线程响应时间
                await Task.Delay(10);
            }
        });
    }

    private string GetDescription(object enumValue)
    {
        return EnumType.GetField(enumValue.ToString() ?? "")
            ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() is DescriptionAttribute descriptionAttribute
                ? descriptionAttribute.Description
                : enumValue.ToString() ?? "";
    }

    public class EnumerationMember
    {
        public string Description { get; set; } = "";
        public object? Value { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}

// 线程安全的 ObservableCollection
public class AsyncObservableCollection<T> : ObservableCollection<T>
{
    private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current
        ?? new SynchronizationContext();

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (SynchronizationContext.Current == _synchronizationContext)
        {
            RaiseCollectionChanged(e);
        }
        else
        {
            _synchronizationContext.Post(RaiseCollectionChanged, e);
        }
    }

    private void RaiseCollectionChanged(object? param)
    {
        if (param is NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (SynchronizationContext.Current == _synchronizationContext)
        {
            RaisePropertyChanged(e);
        }
        else
        {
            _synchronizationContext.Post(RaisePropertyChanged, e);
        }
    }

    private void RaisePropertyChanged(object? param)
    {
        if (param is PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
    }
}