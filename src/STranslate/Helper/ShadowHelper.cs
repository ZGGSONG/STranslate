using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Effects;
using STranslate.Model;

namespace STranslate.Helper;

public class ShadowHelper
{
    private const int Margin = 32;

    private static readonly Collection<ResourceDictionary> AllDict = Application.Current.Resources.MergedDictionaries;

    private static ResourceDictionary _oldResource =
        AllDict.First(x => x.Source.AbsoluteUri == ConstStr.WindowResourcePath);

    private static ResourceDictionary GetResourceDictionary()
    {
        var dict = new ResourceDictionary
        {
            Source = new Uri(ConstStr.WindowResourcePath, UriKind.Absolute)
        };
        return dict;
    }

    private static void UpdateResourceDictionary(ResourceDictionary updateResource)
    {
        AllDict.Remove(_oldResource);
        AllDict.Add(updateResource);
        _oldResource = updateResource;
    }

    private static void AddShadow(ResourceDictionary dict, System.Windows.Style windowStyle)
    {
        if (windowStyle.Setters.FirstOrDefault(setterBase =>
                setterBase is Setter setter && setter.Property == FrameworkElement.MarginProperty) is not Setter
            marginSetter)
        {
            marginSetter = new Setter
            {
                Property = FrameworkElement.MarginProperty,
                Value = new Thickness(Margin, 12, Margin, Margin)
            };
            windowStyle.Setters.Add(marginSetter);
        }
        else
        {
            var baseMargin = (Thickness)marginSetter.Value;
            var newMargin = new Thickness(
                baseMargin.Left + Margin,
                baseMargin.Top + Margin,
                baseMargin.Right + Margin,
                baseMargin.Bottom + Margin);
            marginSetter.Value = newMargin;
        }

        var effect = new Setter(UIElement.EffectProperty, new DropShadowEffect
        {
            Opacity = 0.3,
            ShadowDepth = 12,
            Direction = 270,
            BlurRadius = 30
        });
        windowStyle.Setters.Add(effect);

        UpdateResourceDictionary(dict);
    }

    private static void RemoveShadow(ResourceDictionary dict, System.Windows.Style windowStyle)
    {
        if (windowStyle.Setters.FirstOrDefault(setterBase =>
                setterBase is Setter setter && setter.Property == UIElement.EffectProperty) is Setter effectSetter)
            windowStyle.Setters.Remove(effectSetter);
        if (windowStyle.Setters.FirstOrDefault(setterBase =>
                setterBase is Setter setter && setter.Property == FrameworkElement.MarginProperty) is Setter
            marginSetter)
        {
            var currentMargin = (Thickness)marginSetter.Value;
            var newMargin = new Thickness(
                currentMargin.Left - Margin,
                currentMargin.Top - Margin,
                currentMargin.Right - Margin,
                currentMargin.Bottom - Margin);
            marginSetter.Value = newMargin;
        }

        UpdateResourceDictionary(dict);
    }

    public static void ShadowEffect(bool mainViewShadow)
    {
        var dict = GetResourceDictionary();
        if (dict[ConstStr.WindowResourceName] is not System.Windows.Style windowStyle) return;

        if (mainViewShadow)
            AddShadow(dict, windowStyle);
        else
            RemoveShadow(dict, windowStyle);
    }
}