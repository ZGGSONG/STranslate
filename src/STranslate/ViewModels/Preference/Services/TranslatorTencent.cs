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
using TencentCloud.Common.Profile;
using TencentCloud.Common;
using TencentCloud.Tmt.V20180321.Models;
using TencentCloud.Tmt.V20180321;
using Task = System.Threading.Tasks.Task;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorTencent : ObservableObject, ITranslator
    {
        public TranslatorTencent()
            : this(Guid.NewGuid(), "https://tmt.tencentcloudapi.com", "腾讯翻译君") { }

        public TranslatorTencent(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Tencent,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.TencentService
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
        private IconType _icon = IconType.Baidu;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _appID = string.Empty;

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

        [JsonIgnore]
        [ObservableProperty]
        private TencentRegionEnum _region = TencentRegionEnum.华东地区_上海;

        [JsonIgnore]
        public List<TencentRegionEnum> Resiongs { get; set; } = Enum.GetValues(typeof(TencentRegionEnum)).OfType<TencentRegionEnum>().ToList();

        [JsonIgnore]
        [ObservableProperty]
        private string? _projectId = "0";

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
            if (obj == null) return;

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

        public Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            if (request is RequestModel reqModel)
            {
                // 实例化一个认证对象，入参需要传入腾讯云账户 SecretId 和 SecretKey，此处还需注意密钥对的保密
                // 代码泄露可能会导致 SecretId 和 SecretKey 泄露，并威胁账号下所有资源的安全性。以下代码示例仅供参考，建议采用更安全的方式来使用密钥，请参见：https://cloud.tencent.com/document/product/1278/85305
                // 密钥可前往官网控制台 https://console.cloud.tencent.com/cam/capi 进行获取
                Credential cred = new()
                {
                    SecretId = AppID,
                    SecretKey = AppKey
                };
                // 实例化一个client选项，可选的，没有特殊需求可以跳过
                ClientProfile clientProfile = new();
                // 实例化一个http选项，可选的，没有特殊需求可以跳过
                HttpProfile httpProfile = new()
                {
                    Endpoint = ("tmt.tencentcloudapi.com")
                };
                clientProfile.HttpProfile = httpProfile;

                // 实例化要请求产品的client对象,clientProfile是可选的
                TmtClient client = new(cred, Region.GetDescription(), clientProfile);
                // 实例化一个请求对象,每个接口都会对应一个request对象
                TextTranslateRequest req = new()
                {
                    SourceText = reqModel.Text,
                    Source = reqModel.SourceLang.ToLower(),
                    Target = reqModel.TargetLang.ToLower(),
                    ProjectId = Convert.ToInt64(ProjectId)
                };
                // 返回的resp是一个TextTranslateResponse的实例，与请求对象对应
                TextTranslateResponse resp = client.TextTranslateSync(req);

                var data = resp.TargetText.Length == 0 ? throw new Exception("请求结果为空") : resp.TargetText;

                return Task.FromResult(TranslationResult.Success(data));
            }

            throw new Exception($"请求数据出错: {request}");
        }

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        public ITranslator Clone()
        {
            return new TranslatorTencent
            {
                Identify = this.Identify,
                Type = this.Type,
                IsEnabled = this.IsEnabled,
                Icon = this.Icon,
                Name = this.Name,
                Url = this.Url,
                Data = TranslationResult.Reset,
                AppID = this.AppID,
                AppKey = this.AppKey,
                Icons = this.Icons,
                Region = TencentRegionEnum.华东地区_上海,
                Resiongs = this.Resiongs,
                ProjectId = this.ProjectId,
                IdHide = this.IdHide,
                KeyHide = this.KeyHide,
            };
        }
    }
}