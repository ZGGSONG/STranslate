using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Controls;
using STranslate.Util;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorQwenMt : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorQwenMt()
        : this(Guid.NewGuid(), "https://dashscope.aliyuncs.com", "Qwen-MT")
    {
    }

    public TranslatorQwenMt(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Bailian,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.QwenMtService
    )
    {
        Identify = guid;
        Url = url;
        Name = name;
        Icon = icon;
        AppID = appID;
        AppKey = appKey;
        IsEnabled = isEnabled;
        Type = type;
    }

    #endregion Constructor

    #region Properties

    [JsonIgnore]
    [ObservableProperty]
    private string _model = "qwen-mt-turbo";

    [JsonIgnore]
    [ObservableProperty]
    private ObservableCollection<string> _models =
    [
        "qwen-mt-turbo",
        "qwen-mt-plus"
    ];

    [ObservableProperty] private bool _isEnableTerms;

    /// <summary>
    ///     术语列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<Term> _terms = [];

    [ObservableProperty] private bool _isEnableDomains;

    /// <summary>
    ///     领域提示
    /// </summary>
    [ObservableProperty] private string _domains = "";

    #endregion

    #region Translator Test

    [property: JsonIgnore]
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TestAsync(CancellationToken token)
    {
        var result = "";
        var isCancel = false;
        try
        {
            IsTesting = true;
            var reqModel = new RequestModel("你好", LangEnum.zh_cn, LangEnum.en);
            var ret = await TranslateAsync(reqModel, token);

            result = ret.IsSuccess ? AppLanguageManager.GetString("Toast.VerifySuccess") : AppLanguageManager.GetString("Toast.VerifyFailed");
        }
        catch (OperationCanceledException)
        {
            isCancel = true;
        }
        catch (Exception)
        {
            result = AppLanguageManager.GetString("Toast.VerifyFailed");
        }
        finally
        {
            IsTesting = false;
            if (!isCancel)
                ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Translator Test

    #region Interface Implementation

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(Url) /* || string.IsNullOrEmpty(AppKey)*/)
            throw new Exception("请先完善配置");

        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        UriBuilder uriBuilder = new(Url);

        // 如果路径不是有效的API路径结尾，使用默认路径
        if (uriBuilder.Path == "/")
            uriBuilder.Path = "/compatible-mode/v1/chat/completions";

        // 选择模型
        var a_model = Model.Trim();
        a_model = string.IsNullOrEmpty(a_model) ? "qwen-mt-turbo" : a_model;

        // 构建请求数据
        var translationOptions = new Dictionary<string, object>
        {
            ["source_lang"] = source,
            ["target_lang"] = target
        };

        // 如果启用了术语表，则添加 terms
        if (IsEnableTerms)
        {
            var a_terms = Terms
                //.Where(t => t.IsEnabled)
                .Select(t => new
                {
                    source = t.SourceText,
                    target = t.TargetText
                })
                .ToList();

            translationOptions["terms"] = a_terms;
        }

        if (IsEnableDomains)
        {
            translationOptions["domains"] = Domains;
        }

        var reqData = new
        {
            model = a_model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = req.Text,
                }
            },
            translation_options = translationOptions
        };

        var header = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {AppKey}" }
        };

        var jsonData = JsonConvert.SerializeObject(reqData);

        try
        {
            var resp = await HttpUtil.PostAsync(uriBuilder.Uri.ToString(), jsonData, null, header, token).ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<JObject>(resp)?["choices"]?.FirstOrDefault()?["message"]?["content"]?.ToString() ?? throw new Exception(resp);
            return TranslationResult.Success(data);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == null)
        {
            var msg = $"请检查服务是否可以正常访问: {Name} ({Url}).\n{ex.Message}";
            throw new HttpRequestException(msg);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException is { } innEx)
            {
                var innMsg = JsonConvert.DeserializeObject<JObject>(innEx.Message);
                msg += $" {innMsg?["error"]?["message"]}";
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
            }

            msg = msg.Trim();

            throw new Exception(msg);
        }
    }

    public ITranslator Clone()
    {
        return new TranslatorQwenMt
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            Data = TranslationResult.Reset,
            AppID = AppID,
            AppKey = AppKey,
            AutoExecute = AutoExecute,
            KeyHide = KeyHide,
            Model = Model,
            Models = Models,
            IsExecuting = IsExecuting,
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack,
            IsEnableTerms = IsEnableTerms,
            Terms = Terms.DeepClone(),
            IsEnableDomains = IsEnableDomains,
            Domains = Domains
        };
    }

    /// <summary>
    ///     https://help.aliyun.com/zh/model-studio/machine-translation#14735a54e0rwb
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto", // 自动检测
            LangEnum.zh_cn => "Chinese", // 简体中文
            LangEnum.zh_tw => "Traditional Chinese", // 繁体中文
            LangEnum.yue => "Cantonese", // 粤语
            LangEnum.ja => "Japanese", // 日语
            LangEnum.en => "English", // 英语
            LangEnum.ko => "Korean", // 韩语
            LangEnum.fr => "French", // 法语
            LangEnum.es => "Spanish", // 西班牙语
            LangEnum.ru => "Russian", // 俄语
            LangEnum.de => "German", // 德语
            LangEnum.it => "Italian", // 意大利语
            LangEnum.tr => "Turkish", // 土耳其语
            LangEnum.pt_pt => "Portuguese", // 葡萄牙语（葡萄牙）
            LangEnum.pt_br => "Portuguese", // 葡萄牙语（巴西）
            LangEnum.vi => "Vietnamese", // 越南语
            LangEnum.id => "Indonesian", // 印度尼西亚语
            LangEnum.th => "Thai", // 泰语
            LangEnum.ms => "Malay", // 马来语
            LangEnum.ar => "Arabic", // 阿拉伯语
            LangEnum.hi => "Hindi", // 印地语
            LangEnum.mn_cy => null, // 不支持（蒙古语-西里尔）
            LangEnum.mn_mo => null, // 不支持（蒙古语-蒙文）
            LangEnum.km => "Khmer", // 高棉语
            LangEnum.nb_no => "Norwegian Bokmål", // 书面挪威语
            LangEnum.nn_no => "Norwegian Nynorsk", // 新挪威语
            LangEnum.fa => "Western Persian", // 西波斯语
            LangEnum.sv => "Swedish", // 瑞典语
            LangEnum.pl => "Polish", // 波兰语
            LangEnum.nl => "Dutch", // 荷兰语
            LangEnum.uk => "Ukrainian", // 乌克兰语
            _ => null
        };
    }

    #endregion Interface Implementation

    #region RelayCommands

    [RelayCommand]
    private void Add()
    {
        var term = new Term();
        Terms.Add(term);
    }

    [RelayCommand]
    private void Delete(Term? term)
    {
        if (term is null || !Terms.Contains(term))
            return;
        Terms.Remove(term);
    }

    [RelayCommand]
    private void Clear()
    {
        var ret = MessageBox_S.Show(AppLanguageManager.GetString("MessageBox.Terms.Clear"), AppLanguageManager.GetString("MessageBox.Tip"), MessageBoxButton.OKCancel);
        if (ret == MessageBoxResult.OK)
            Terms.Clear();
    }

    [RelayCommand]
    private void Export()
    {
        if (Terms.Count == 0)
        {
            ToastHelper.Show(AppLanguageManager.GetString("Toast.Terms.EmptyList"), WindowType.Preference);
            return;
        }

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV文件(*.csv)|*.csv|所有文件(*.*)|*.*",
            FileName = $"QwenMt_Terms_{DateTime.Now:yyyyMMddHHmmss}.csv",
            Title = AppLanguageManager.GetString("Toast.Terms.ExportTitle")
        };

        if (saveFileDialog.ShowDialog() != true)
            return;

        try
        {
            ExportTermsToCsv(saveFileDialog.FileName);
            ToastHelper.Show(string.Format(AppLanguageManager.GetString("Toast.Terms.ExportSuccess"), Terms.Count), WindowType.Preference);
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"导出失败: {ex.Message}", ex);
            ToastHelper.Show(AppLanguageManager.GetString("Toast.Terms.ExportFailed"), WindowType.Preference);
        }
    }

    [RelayCommand]
    private void Import()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "CSV文件(*.csv)|*.csv|所有文件(*.*)|*.*",
            Title = AppLanguageManager.GetString("Toast.Terms.ImportTitle")
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        try
        {
            var importedTerms = ImportTermsFromCsv(openFileDialog.FileName);

            if (importedTerms.Count == 0)
            {
                ToastHelper.Show(AppLanguageManager.GetString("Toast.Terms.NoValidData"), WindowType.Preference);
                return;
            }

            // 询问导入方式
            var result = ShowImportDialog(importedTerms.Count);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    // 替换现有术语
                    Terms.Clear();
                    foreach (var term in importedTerms)
                        Terms.Add(term);
                    ToastHelper.Show(string.Format(AppLanguageManager.GetString("Toast.Terms.ImportReplace"), importedTerms.Count), WindowType.Preference);
                    break;

                case MessageBoxResult.No:
                    // 追加到现有术语
                    foreach (var term in importedTerms)
                        Terms.Add(term);
                    ToastHelper.Show(string.Format(AppLanguageManager.GetString("Toast.Terms.ImportAppend"), importedTerms.Count), WindowType.Preference);
                    break;

                case MessageBoxResult.Cancel:
                    return;
            }
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"导入失败: {ex.Message}", ex);
            ToastHelper.Show(AppLanguageManager.GetString("Toast.Terms.ImportFailed"), WindowType.Preference);
        }
    }

    /// <summary>
    ///     显示导入确认对话框
    /// </summary>
    /// <param name="count">导入的术语数量</param>
    /// <returns>用户选择结果</returns>
    private MessageBoxResult ShowImportDialog(int count)
    {
        var message = string.Format(AppLanguageManager.GetString("MessageBox.Terms.ImportConfirm"), count);
        var title = AppLanguageManager.GetString("MessageBox.Terms.ImportTitle");

        return MessageBox_S.Show(
            message,
            title,
            MessageBoxButton.YesNoCancel);
    }

    #endregion

    #region Private Methods - CSV Import/Export

    /// <summary>
    /// 导出术语到 CSV 文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    private void ExportTermsToCsv(string filePath)
    {
        var csvContent = new StringBuilder();

        // 添加 UTF-8 BOM 以确保 Excel 正确显示中文
        csvContent.Append('\ufeff');

        // 添加 CSV 头部
        csvContent.AppendLine("Source,Target");

        // 导出术语数据
        foreach (var term in Terms)
        {
            var sourceText = EscapeCsvField(term.SourceText);
            var targetText = EscapeCsvField(term.TargetText);
            csvContent.AppendLine($"{sourceText},{targetText}");
        }

        File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// 从 CSV 文件导入术语
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>导入的术语列表</returns>
    private List<Term> ImportTermsFromCsv(string filePath)
    {
        var csvContent = File.ReadAllText(filePath, Encoding.UTF8);
        return ParseCsvContent(csvContent);
    }

    /// <summary>
    /// 解析 CSV 内容并转换为 Term 对象列表
    /// </summary>
    /// <param name="csvContent">CSV 文件内容</param>
    /// <returns>Term 对象列表</returns>
    private List<Term> ParseCsvContent(string csvContent)
    {
        var terms = new List<Term>();

        if (string.IsNullOrWhiteSpace(csvContent))
            return terms;

        // 移除 BOM 标记
        if (csvContent.StartsWith('\ufeff'))
            csvContent = csvContent[1..];

        var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var isFirstLine = true;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // 跳过可能的头部行
            if (isFirstLine && IsHeaderLine(line))
            {
                isFirstLine = false;
                continue;
            }
            isFirstLine = false;

            var parsedFields = ParseCsvLine(line);
            if (parsedFields.Count >= 2)
            {
                var sourceText = parsedFields[0].Trim();
                var targetText = parsedFields[1].Trim();

                // 跳过完全空的术语条目
                if (!string.IsNullOrWhiteSpace(sourceText) || !string.IsNullOrWhiteSpace(targetText))
                {
                    terms.Add(new Term
                    {
                        SourceText = sourceText,
                        TargetText = targetText
                    });
                }
            }
        }

        return terms;
    }

    /// <summary>
    /// 判断是否为 CSV 头部行
    /// </summary>
    /// <param name="line">行内容</param>
    /// <returns>是否为头部行</returns>
    private static bool IsHeaderLine(string line)
    {
        var trimmedLine = line.Trim().ToLowerInvariant();
        return trimmedLine == "source,target" ||
               trimmedLine == "源语言,目标语言" ||
               trimmedLine == "原文,译文" ||
               trimmedLine == "\"source\",\"target\"";
    }

    /// <summary>
    /// 解析单行 CSV 数据，正确处理引号和逗号
    /// </summary>
    /// <param name="csvLine">CSV 行数据</param>
    /// <returns>字段列表</returns>
    private static List<string> ParseCsvLine(string csvLine)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < csvLine.Length; i++)
        {
            var currentChar = csvLine[i];

            if (currentChar == '"')
            {
                if (inQuotes && i + 1 < csvLine.Length && csvLine[i + 1] == '"')
                {
                    // 转义的引号（两个连续的引号）
                    currentField.Append('"');
                    i++; // 跳过下一个引号
                }
                else
                {
                    // 切换引号状态
                    inQuotes = !inQuotes;
                }
            }
            else if (currentChar == ',' && !inQuotes)
            {
                // 字段分隔符
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                // 普通字符
                currentField.Append(currentChar);
            }
        }

        // 添加最后一个字段
        fields.Add(currentField.ToString());
        return fields;
    }

    /// <summary>
    /// 转义 CSV 字段，处理包含逗号、引号或换行符的情况
    /// </summary>
    /// <param name="field">要转义的字段</param>
    /// <returns>转义后的字段</returns>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // 如果字段包含逗号、引号或换行符，需要用引号包围并转义内部引号
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            // 转义引号（双引号变成两个双引号）
            var escapedField = field.Replace("\"", "\"\"");
            // 用引号包围整个字段
            return $"\"{escapedField}\"";
        }

        return field;
    }

    #endregion
}