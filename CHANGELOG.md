## 更新

- 新增: LLM OCR 底层更新, 取消依赖结构化数据返回
- 新增: LLM 配置新增 API 接口描述
- 新增: 添加任务计划程序启动脚本（跳过UAC）
- 优化: Yandex图标大小
- 优化: GHProxy代理修改至`https://gh-proxy.com/`
- 优化: 热键设置界面描述优化
- 优化: OpenAI(或兼容OpenAI) 文本翻译服务推理模型输出
- 优化: LLM OCR Prompt 感谢 [gemini-ocr](https://github.com/skitsanos/gemini-ocr)
- 优化: 添加文本翻译服务的显示效果
- 优化: Gemini文本翻译服务(gemini-2.0-flash-exp)最后一项总是换行
- 修复: 在手动指定语种后请求服务出现错误的问题
- 修复: 添加或删除文本翻译服务时列表显示不跟随的问题
- 修复: 恢复配置是翻译配置页面中配置页面丢失和字典服务恢复失败的问题
- 修复: 解决LLM文本翻译服务配置页面 API 密钥无法显示的问题
- 其他: 移除header服务列表右键切换prompt
- 其他: 优化项目结构

**完整更新日志:** [1.3.1.120...1.3.2.305](https://github.com/ZGGSONG/STranslate/compare/1.3.1.120...1.3.2.305)

## 离线数据

[123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

> 离线数据包含: [简明英汉词典数据包](https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip)  [PaddleOCR数据包(v4.3)](https://github.com/ZGGSONG/STranslate/releases/download/0.01/stranslate_paddleocr_data_v4.3.zip) 等

> 另: OCR部分软件内置`微信OCR`、词典部分软件内置了`必应`、`金山`词典，上述离线数据包配置较为繁琐，可直接使用软件内置服务