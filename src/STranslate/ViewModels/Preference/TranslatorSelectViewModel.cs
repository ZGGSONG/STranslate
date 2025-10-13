using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;
using STranslate.ViewModels.Base;
using STranslate.ViewModels.Preference.Translator;

namespace STranslate.ViewModels.Preference;

public partial class TranslatorSelectViewModel : WindowVMBase
{
    [ObservableProperty]
    private BindingList<ITranslator> _translators = [];

    public ITranslator? SelectedTranslator { get; set; }

    public override void Close(Window win)
    {
        win.DialogResult = false;
        base.Close(win);
    }

    internal void UpdateList(BindingList<ITranslator> translators)
    {
        if (Translators.Count != 0)
            return;

        foreach (var item in translators)
        {
            Translators.Add(item);
        }
    }

    [RelayCommand]
    private void SelectItem(List<object> list)
    {
        if (list.Count != 2) return;

        var translator = list.First();

        SelectedTranslator = translator switch
        {
            TranslatorSTranslate stranslate => stranslate.Clone(),
            TranslatorGoogleBuiltin api => api.Clone(),
            TranslatorBaidu baidu => baidu.Clone(),
            TranslatorMicrosoft bing => bing.Clone(),
            TranslatorOpenAI openAi => openAi.Clone(),
            TranslatorGemini gemini => gemini.Clone(),
            TranslatorTencent tencent => tencent.Clone(),
            TranslatorAli ali => ali.Clone(),
            TranslatorYoudao youdao => youdao.Clone(),
            TranslatorNiutrans niutrans => niutrans.Clone(),
            TranslatorCaiyun caiyun => caiyun.Clone(),
            TranslatorVolcengine volcengine => volcengine.Clone(),
            TranslatorEcdict ecdict => ecdict.Clone(),
            TranslatorChatglm chatglm => chatglm.Clone(),
            TranslatorOllama ollama => ollama.Clone(),
            TranslatorBaiduBce baiduBce => baiduBce.Clone(),
            TranslatorDeepL deepl => deepl.Clone(),
            TranslatorAzureOpenAI azureopenai => azureopenai.Clone(),
            TranslatorClaude claude => claude.Clone(),
            TranslatorDeepSeek deepSeek => deepSeek.Clone(),
            TranslatorKingSoftDict kingsoftdict => kingsoftdict.Clone(),
            TranslatorBingDict bingdict => bingdict.Clone(),
            TranslatorDeepLX deeplx => deeplx.Clone(),
            TranslatorYandexBuiltIn yandex => yandex.Clone(),
            TranslatorMicrosoftBuiltin microsoftBuiltin => microsoftBuiltin.Clone(),
            TranslatorDeerAPI deerapi => deerapi.Clone(),
            TranslatorTransmartBuiltIn transmartbuiltin => transmartbuiltin.Clone(),
            TranslatorOpenRouter openrouter => openrouter.Clone(),
            TranslatorQwenMt qwenmt => qwenmt.Clone(),
            TranslatorMTranServer mtranserver => mtranserver.Clone(),
            //TODO: 新接口需要适配
            _ => throw new InvalidOperationException($"Unsupported service type: {translator.GetType().Name}")
        };

        ((Window)list.Last()).DialogResult = true;
    }
}