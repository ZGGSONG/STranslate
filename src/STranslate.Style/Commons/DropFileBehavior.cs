using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace STranslate.Style.Commons;

public class DropFileBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty FilesProperty = DependencyProperty.Register(
        nameof(Data),
        typeof(string[]),
        typeof(DropFileBehavior),
        new UIPropertyMetadata(null)
    );

    public string[]? Data
    {
        get => (string[]?)GetValue(FilesProperty);
        set => SetValue(FilesProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.AllowDrop = true;
        AssociatedObject.Drop += DropHandler;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Drop -= DropHandler;
    }

    private void DropHandler(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop)) Data = e.Data.GetData(DataFormats.FileDrop) as string[];
    }
}