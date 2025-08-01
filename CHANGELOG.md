## 更新

- 新增: OpenRouter服务
- 新增: QwenMT服务
- 新增: MTranServer服务
- 新增: 墨墨背单词生词本
- 新增: 主界面服务标题及Prompt长度调整选项
- 新增: 管理员(跳过UAC)启动软件选项
- 新增: 软件内更新添加进度条显示
- 优化: 智普AI服务支持可选启动思考模式(仅 GLM-4.5 及以上模型支持此参数配置,控制大模型是否开启思维链)
- 优化: UI显示效果
- 修复: OpenAI/Gemini OCR 图片格式匹配错误的问题

> 该版本内部集成任务计划启动以跳过UAC弹窗确认, 用户可以在软件内设置是否启用管理员权限启动软件, 开机自启由软件内 `开机自启动` 控制
> **如果先前通过 `install_startup.bat` 创建了任务计划, 更新前请运行软件内 `uninstall_startup.bat` 删除旧的任务计划程序以避免异常情况**

**完整更新日志:** [1.5.3.711...1.5.4.801](https://github.com/ZGGSONG/STranslate/compare/1.5.3.711...1.5.4.801)

## 离线数据

[123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

> 离线数据包含: [简明英汉词典数据包](https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip)  [PaddleOCR数据包(v4.3)](https://github.com/ZGGSONG/STranslate/releases/download/0.01/stranslate_paddleocr_data_v4.3.zip) 等

> 另: OCR部分软件内置`微信OCR`(1.5.0.402版本开始内置微信OCR数据包，无需依赖外部微信)、词典部分软件内置了`必应`、`金山`词典，上述离线数据包配置较为繁琐，可直接使用软件内置服务