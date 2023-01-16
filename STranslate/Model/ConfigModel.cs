using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Model
{
    public class ConfigModel
    {
        public ConfigModel()
        {
        }

        public ConfigModel InitialConfig()
        {
            return new ConfigModel
            {
                WordPickupInterval = 200,
                IsBright = true,
                SourceLanguage = LanguageEnum.AUTO.GetDescription(),
                TargetLanguage = LanguageEnum.AUTO.GetDescription(),
                SelectServer = 0,
                Servers = new Server[]
                {
                    new Server
                    {
                        Name = "zggsong",
                        Api = "https://zggsong.cn/tt"
                    },
                    new Server
                    {
                        Name = "zu1k",
                        Api = "https://deepl.deno.dev/translate"
                    },
                    new Server
                    {
                        Name = "local",
                        Api = "http://127.0.0.1:8000/translate"
                    }
                }
            };
        }

        /// <summary>
        /// 是否亮色模式
        /// </summary>
        [JsonProperty("wordPickupInterval")]
        public double WordPickupInterval { get; set; }
        /// <summary>
        /// 是否亮色模式
        /// </summary>
        [JsonProperty("isBright")]
        public bool IsBright { get; set; }

        [JsonProperty("sourceLanguage")]
        public string SourceLanguage { get; set; }

        [JsonProperty("targetLanguage")]
        public string TargetLanguage { get; set; }

        [JsonProperty("selectServer")]
        public int SelectServer { get; set; }

        /// <summary>
        /// 服务
        /// </summary>
        [JsonProperty("servers")]
        public Server[] Servers { get; set; }

    }

    public class Server
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("api")]
        public string Api { get; set; }
    }
}
