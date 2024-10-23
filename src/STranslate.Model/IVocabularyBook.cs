using System.ComponentModel;

namespace STranslate.Model;

public interface IVocabularyBook : INotifyPropertyChanged //需要继承INotifyPropertyChanged，否则切换属性时无法通知
{
    Guid Identify { get; set; }

    VocabularyBookType Type { get; set; }

    IconType Icon { get; set; }

    bool IsEnabled { get; set; }

    string Name { get; set; }

    string Url { get; set; }

    string BookName { get; set; }

    string AppID { get; set; }

    string AppKey { get; set; }

    Task<bool> CheckAsync(CancellationToken token);

    Task<bool> ExecuteAsync(string text, CancellationToken token);

    IVocabularyBook Clone();
}

public class VocabularyBookCollection<T> : BindingList<T> where T : class, IVocabularyBook
{
    protected override void OnListChanged(ListChangedEventArgs e)
    {
        base.OnListChanged(e);

        // 当项被添加或者属性改变时，检查 IsEnabled 属性
        if (e.ListChangedType != ListChangedType.ItemAdded && e is not
            {
                ListChangedType: ListChangedType.ItemChanged, PropertyDescriptor.Name: nameof(IVocabularyBook.IsEnabled)
            }) return;

        var changedItem = this[e.NewIndex];
        if (!changedItem.IsEnabled) return;
        // 设置其他所有项的 IsEnabled 为 false
        foreach (var item in this)
            if (!ReferenceEquals(item, changedItem) && item.IsEnabled)
                item.IsEnabled = false;
    }

    public VocabularyBookCollection<IVocabularyBook> DeepCopy()
    {
        var copiedList = new VocabularyBookCollection<IVocabularyBook>();

        foreach (var item in this)
        {
            var newItem = item.Clone();
            copiedList.Add(newItem);
        }

        return copiedList;
    }
}