## 更新

- 修复: QwenMT 领域提示无法正常加载的问题
- 修复: ChatGLM 关闭思考无法保存的问题

> 该版本内部集成任务计划启动以跳过UAC弹窗确认, 用户可以在软件内设置是否启用管理员权限启动软件, 开机自启由软件内 `开机自启动` 控制
> **如果先前通过 `install_startup.bat` 创建了任务计划, 更新前请运行软件内 `uninstall_startup.bat` 删除旧的任务计划程序以避免异常情况**

**完整更新日志:** [1.5.4.801...1.5.5.802](https://github.com/ZGGSONG/STranslate/compare/1.5.4.801...1.5.5.802)

## 离线数据

[123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

> 离线数据包含: [简明英汉词典数据包](https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip)  [PaddleOCR数据包(v4.3)](https://github.com/ZGGSONG/STranslate/releases/download/0.01/stranslate_paddleocr_data_v4.3.zip) 等

> 另: OCR部分软件内置`微信OCR`(1.5.0.402版本开始内置微信OCR数据包，无需依赖外部微信)、词典部分软件内置了`必应`、`金山`词典，上述离线数据包配置较为繁琐，可直接使用软件内置服务