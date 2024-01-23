using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorBaidu : ObservableObject, ITranslator
    {
        public TranslatorBaidu()
            : this(Guid.NewGuid(), "https://fanyi-api.baidu.com/api/trans/vip/translate", "Baidu") { }

        public TranslatorBaidu(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Baidu,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.BaiduService
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
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _AppID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _appKey = string.Empty;

        [JsonIgnore]
        public object _data = string.Empty;

        [JsonIgnore]
        public object Data
        {
            get => _data;
            set
            {
                if (_data != value)
                {
                    OnPropertyChanging(nameof(Data));
                    _data = value;
                    OnPropertyChanged(nameof(Data));
                }
            }
        }

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

        #region Show/Hide Encrypt Info

        [JsonIgnore]
        private bool _idHide = true;

        [JsonIgnore]
        public bool IdHide
        {
            get => _idHide;
            set
            {
                if (_idHide != value)
                {
                    OnPropertyChanging(nameof(IdHide));
                    _idHide = value;
                    OnPropertyChanged(nameof(IdHide));
                }
            }
        }

        [JsonIgnore]
        private bool _keyHide = true;

        [JsonIgnore]
        public bool KeyHide
        {
            get => _keyHide;
            set
            {
                if (_keyHide != value)
                {
                    OnPropertyChanging(nameof(KeyHide));
                    _keyHide = value;
                    OnPropertyChanged(nameof(KeyHide));
                }
            }
        }

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

        public async Task<object> TranslateAsync(object request, CancellationToken token)
        {
            if (request is RequestBaidu rb)
            {
                var req = new Dictionary<string, string>
                {
                    { "q", rb.Text },
                    { "from", rb.From.ToLower() },
                    { "to", rb.TO.ToLower() },
                    { "appid", rb.AppId },
                    { "salt", rb.Salt },
                    { "sign", rb.Sign }
                };

                string resp = await HttpUtil.GetAsync(Url, req, token);
                if (string.IsNullOrEmpty(resp))
                    throw new Exception("请求结果为空");

                var ret = JsonConvert.DeserializeObject<ResponseBaidu>(resp ?? "");

                //如果出错就将整个返回信息写入取值处
                if (ret is null || string.IsNullOrEmpty(ret.TransResult?.FirstOrDefault()?.Dst))
                {
                    ret = new ResponseBaidu { TransResult = [new TransResult { Dst = resp! }] };
                }
                return Task.FromResult<object>(ret);
            }

            return Task.FromResult<object>(new ResponseBaidu { TransResult = [new TransResult { Dst = "请求数据出错..." }] });
        }

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}