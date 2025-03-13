## 更新

- 新增: 添加国际化支持，目前支持中文、繁体中文和英文
- 新增: 添加支持有道OCR
- 新增: Prompt 新增快捷添加副本功能
- 新增: 文本翻译服务新增右键快捷创建副本及删除功能
- 新增: 添加全局请求超时时间配置功能
- 优化: 换行符处理优化功能(取词时换行/自动净化)合并到换行枚举中(不处理换行/移除多余换行/移除所有换行)
- 优化: 更新热键指令，更新回译快速切换使用`Ctrl`组合键
- 优化: 识别语种时取消翻译清空识别内容
- 优化: 服务配置页面导航交互
- 修复: 有道翻译语种代码部分错误的情况
- 修复: LLM Prompt对话框角色指定不正确的问题
- 修复: 文本翻译恢复服务影响替换翻译配置的问题
- 修复: 撤销有时不生效的问题

**完整更新日志:** [1.3.2.305...1.4.1.313](https://github.com/ZGGSONG/STranslate/compare/1.3.2.305...1.4.1.313)

## 离线数据

[123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

> 离线数据包含: [简明英汉词典数据包](https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip)  [PaddleOCR数据包(v4.3)](https://github.com/ZGGSONG/STranslate/releases/download/0.01/stranslate_paddleocr_data_v4.3.zip) 等

> 另: OCR部分软件内置`微信OCR`、词典部分软件内置了`必应`、`金山`词典，上述离线数据包配置较为繁琐，可直接使用软件内置服务