using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Model
{

    public class Hotkeys
    {
        [JsonProperty("inputTranslate")]
        public InputTranslate InputTranslate { get; set; }

        [JsonProperty("crosswordTranslate")]
        public CrosswordTranslate CrosswordTranslate { get; set; }

        [JsonProperty("screenShotTranslate")]
        public ScreenShotTranslate ScreenShotTranslate { get; set; }

        [JsonProperty("openMainWindow")]
        public OpenMainWindow OpenMainWindow { get; set; }
    }
    public class InputTranslate
    {
        public byte Modifiers { get; set; }
        public int Key { get; set; }
        public String Text { get; set; }
        public bool Conflict { get; set; }
    }
    public class CrosswordTranslate
    {
        public byte Modifiers { get; set; }
        public int Key { get; set; }
        public String Text { get; set; }
        public bool Conflict { get; set; }
    }
    public class ScreenShotTranslate
    {
        public byte Modifiers { get; set; }
        public int Key { get; set; }
        public String Text { get; set; }
        public bool Conflict { get; set; }
    }
    public class OpenMainWindow
    {
        public byte Modifiers { get; set; }
        public int Key { get; set; }
        public String Text { get; set; }
        public bool Conflict { get; set; }
    }
    public class Server
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("api")]
        public string Api { get; set; }
    }
    public class ConfigModel
    {
        /// <summary>
        /// 最大历史记录数量
        /// </summary>
        [JsonProperty("maxHistoryCount")]
        public int MaxHistoryCount { get; set; }
        /// <summary>
        /// 自动识别语种标度
        /// </summary>
        [JsonProperty("autoScale")]
        public double AutoScale { get; set; }
        /// <summary>
        /// 取词间隔
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

        /// <summary>
        /// 热键
        /// </summary>
        [JsonProperty("hotkeys")]
        public Hotkeys Hotkeys { get; set; }


        public ConfigModel()
        {
        }

        public ConfigModel InitialConfig()
        {
            return new ConfigModel
            {
                MaxHistoryCount = 100,
                AutoScale = 0.8,
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
                },
                Hotkeys = new Hotkeys
                {
                    InputTranslate = new InputTranslate
                    {
                        Modifiers = 1,
                        Key = 65,
                        Text = "Alt + A",
                        Conflict = false,
                    },
                    CrosswordTranslate = new CrosswordTranslate
                    {
                        Modifiers = 1,
                        Key = 68,
                        Text = "Alt + D",
                        Conflict = false,
                    },
                    ScreenShotTranslate = new ScreenShotTranslate
                    {
                        Modifiers = 1,
                        Key = 83,
                        Text = "Alt + S",
                        Conflict = false,
                    },
                    OpenMainWindow = new OpenMainWindow
                    {
                        Modifiers = 1,
                        Key = 71,
                        Text = "Alt + G",
                        Conflict = false,
                    },
                }
            };
        }
    }
}
