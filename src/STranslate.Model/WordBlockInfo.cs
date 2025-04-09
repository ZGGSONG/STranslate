using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;

namespace STranslate.Model;

public partial class WordBlockInfo : ObservableObject
{
    [ObservableProperty]
    public string _text = string.Empty;

    [ObservableProperty]
    public Point _position;

    [ObservableProperty]
    public int _width;

    [ObservableProperty]
    public int _height;
    
    [ObservableProperty]
    public double _fontSize;
}