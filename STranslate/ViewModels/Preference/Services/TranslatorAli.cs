using AlibabaCloud.SDK.Alimt20181012;
using AlibabaCloud.SDK.Alimt20181012.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorAli : ObservableObject, ITranslator
    {
        public TranslatorAli()
            : this(Guid.NewGuid(), "https://mt.cn-hangzhou.aliyuncs.com", "阿里翻译") { }

        public TranslatorAli(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Ali,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.AliService
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

        [ObservableProperty]
        private Guid _identify = Guid.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private ServiceType _type = 0;

        [JsonIgnore]
        [ObservableProperty]
        public bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.Ali;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _AppID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _appKey = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        public TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

        #region Show/Hide Encrypt Info

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _idHide = true;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _keyHide = true;

        private void ShowEncryptInfo(string? obj)
        {
            if (obj == null)
                return;

            if (obj.Equals(nameof(AppID)))
            {
                IdHide = !IdHide;
            }
            else if (obj.Equals(nameof(AppKey)))
            {
                KeyHide = !KeyHide;
            }
        }

        private RelayCommand<string>? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand<string> ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand<string>(new Action<string?>(ShowEncryptInfo));

        #endregion Show/Hide Encrypt Info

        /**
         * 使用AK&SK初始化账号Client
         * @param accessKeyId
         * @param accessKeySecret
         * @param url
         * @return Client
         * @throws Exception
         */

        public static Client CreateClient(string accessKeyId, string accessKeySecret, string url)
        {
            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // 删除 "https://"
                url = new string(url.Skip("https://".Length).ToArray());
            }

            AlibabaCloud.OpenApiClient.Models.Config config =
                new()
                {
                    // 必填，您的 AccessKey ID
                    AccessKeyId = accessKeyId,
                    // 必填，您的 AccessKey Secret
                    AccessKeySecret = accessKeySecret,
                    // Endpoint 请参考 https://api.aliyun.com/product/alimt
                    Endpoint = url
                };
            return new Client(config);
        }

        public Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            if (request is RequestModel reqModel)
            {
                // 请确保代码运行环境设置了环境变量 ALIBABA_CLOUD_ACCESS_KEY_ID 和 ALIBABA_CLOUD_ACCESS_KEY_SECRET。
                // 工程代码泄露可能会导致 AccessKey 泄露，并威胁账号下所有资源的安全性。以下代码示例使用环境变量获取 AccessKey 的方式进行调用，仅供参考，建议使用更安全的 STS 方式，更多鉴权访问方式请参见：https://help.aliyun.com/document_detail/378671.html
                Client client = CreateClient(AppID, AppKey, Url);
                TranslateGeneralRequest translateGeneralRequest =
                    new()
                    {
                        FormatType = "text",
                        SourceLanguage = reqModel.SourceLang.ToLower(),
                        TargetLanguage = reqModel.TargetLang.ToLower(),
                        SourceText = reqModel.Text,
                        Scene = "general",
                    };
                AlibabaCloud.TeaUtil.Models.RuntimeOptions runtime = new();

                TranslateGeneralResponse resp = client.TranslateGeneralWithOptions(translateGeneralRequest, runtime);

                var data = resp.Body.Data.Translated;
                data = data.Length == 0 ? throw new Exception("请求结果为空") : data;

                return Task.FromResult(TranslationResult.Success(data));
            }

            throw new Exception($"请求数据出错: {request}");
        }

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
