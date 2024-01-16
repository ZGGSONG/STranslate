using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorOpenAI : ObservableObject, ITranslator
    {
        public TranslatorOpenAI()
            : this(Guid.NewGuid(), "https://api.openai.com/v1", "OpenAI") { }

        public TranslatorOpenAI(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.OpenAI,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.OpenAIService
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
        private IconType _icon = IconType.Bing;

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

        [Obsolete]
        public async Task<object> TranslateAsync(object request, CancellationToken token)
        {
            try
            {
                if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(AppKey))
                    throw new Exception("请先完善配置");

                if (!Url.EndsWith("completions"))
                {
                    Url = Url.TrimEnd('/') + "/completions";
                }

                if (request != null)
                {
                    var jsonData = JsonConvert.SerializeObject(request);

                    // 构建请求
                    var client = new HttpClient(new SocketsHttpHandler());
                    var req = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(Url),
                        Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                    };
                    req.Headers.Add("Authorization", $"Bearer {AppKey}");

                    // 发送请求 
                    using var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
                    // 获取响应流
                    using var responseStream = await response.Content.ReadAsStreamAsync(token);
                    using var reader = new System.IO.StreamReader(responseStream);
                    // 逐行读取并输出结果
                    while (!reader.EndOfStream || token.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync(token);

                        if (string.IsNullOrEmpty(line?.Trim())) continue;

                        var preprocessString = line.Replace("data:", "").Trim();

                        // 结束标记
                        if (preprocessString.Equals("[DONE]")) break;

                        // 解析JSON数据
                        var parsedData = JsonConvert.DeserializeObject<JObject>(preprocessString);

                        if (parsedData is null) continue;

                        // 提取content的值
                        var contentValue = parsedData["choices"]?.FirstOrDefault()?["delta"]?["content"]?.ToString();

                        if (string.IsNullOrEmpty(contentValue)) continue;

                        // 输出
                        Data += contentValue;
                        //Debug.Write(contentValue);
                    }
                }

            }
            catch (Exception ex)
            {
                Data = ex.Message;
            }

            return Task.FromResult<string?>(null);
        }
    }
}