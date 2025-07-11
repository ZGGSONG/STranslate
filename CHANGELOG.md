## 更新

- 新增: 增加了换行处理（用非空格替换换行符），并修复了配置项描述不正确的问题
- 新增: 增加阿里百炼图标
- 新增: 内置 Transmart 翻译服务
- 新增: 键盘快捷键(Ctrl+Shift+R)调整翻译服务顺序
- 优化: 内置 Yandex 翻译服务，支持繁体中文和粤语（默认处理为中文）
- 优化: 优化翻译服务中创建副本的逻辑，使之更加符合日常习惯
- 优化: 优化 OCR 窗口的显示位置（在哪边截图就出现在哪边显示器上）
- 优化: 主界面显示效果
- 优化: 将 Deepseek 服务 UI 的温度限制调整为 0-2
- 优化: 优化OpenAI服务 - Groq Q Wen推理内容隐藏
- 优化: 添加LLM模型列表和保存功能
- 优化: Gemini API 解析 避免部分消息无法解析
- 优化: 翻译服务配置页面添加服务类型显示
- 优化: 重新点击关闭弹出列表窗口的行为，感谢 @vickyqu115
- 优化: 调整主界面服务名称最大宽度和 Prompt 按钮宽度
- 优化: 优化历史记录的存储内容以及空结果的取缓存逻辑
- 优化: 服务页面快捷键保存配置时，仅保存当前页面配置
- 修复: 保存服务配置并再次点击撤销后，服务配置无法正确显示的问题
- 修复: 无法保存词库的问题
- 修复: 在linux上编译错误的问题，感谢 @hnalpha323
- 修复: 部分用户多显示器截图时会出现截图区域不正确的问题
- 修复: 腾讯识别服务错误的问题
- 修复: 程序启动未使用设置的超时时间
- 修复: 在主界面中显示/隐藏语言控制时的提示内容

> 软件根目录脚本 `install_startup.bat` 和 `uninstall_startup.bat` 用来创建和删除任务计划程序，主要是为了以管理员权限启动软件而不经过`UAC弹窗`确认（也可以创建普通权限启动）
> **该脚本与软件内设置`开机自启动`不兼容，脚本创建任务计划程序后建议关闭软件内`开机启动项`配置**

**完整更新日志:** [1.5.2.408...1.5.3.711](https://github.com/ZGGSONG/STranslate/compare/1.5.2.408...1.5.3.711)

## 合作推广

🛠️ **官方API合作伙伴**  

[DeerAPI](https://api.deerapi.com/register?aff=j5dj) - AI聚合平台，一键调用500+模型，7折特惠，最新GPT4o、Grok 3、Gemini 2.5pro全支持！

[点击注册](https://api.deerapi.com/register?aff=j5dj)享免费试用额度，也能支持软件长久发展


## 离线数据

[123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

> 离线数据包含: [简明英汉词典数据包](https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip)  [PaddleOCR数据包(v4.3)](https://github.com/ZGGSONG/STranslate/releases/download/0.01/stranslate_paddleocr_data_v4.3.zip) 等

> 另: OCR部分软件内置`微信OCR`(1.5.0.402版本开始内置微信OCR数据包，无需依赖外部微信)、词典部分软件内置了`必应`、`金山`词典，上述离线数据包配置较为繁琐，可直接使用软件内置服务