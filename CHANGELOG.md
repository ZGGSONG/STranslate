## 注意

拆分原 `自建服务` 为 `DeepLX服务` 和 `Google服务`(无需配置), 为了翻译准确性, 强烈建议删除原有自带Google翻译服务并重新添加

## 更新

- 新增: Gemini OCR
- 新增: 可选通过剪贴板实现替换翻译和插入结果
- 优化: 拆分DeepLX服务并完善语种列表
- 优化: 拆分Google翻译内置服务并完善语种列表
- 优化: 优化默认配置中字体大小及无效服务
- 修复: 更改指针大小并执行静默OCR等功能后指针变模糊的问题
- 修复: 用户在等待翻译结果时修改内容导致存数据库出错的问题

**完整更新日志:** [1.2.12.1222...1.3.0.0107](https://github.com/ZGGSONG/STranslate/compare/1.2.12.1222...1.3.0.0107)

## 离线数据

[123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

> 离线数据包含: [简明英汉词典数据包](https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip)  [PaddleOCR数据包(v4.3)](https://github.com/ZGGSONG/STranslate/releases/download/0.01/stranslate_paddleocr_data_v4.3.zip) 等

> 另: OCR部分软件内置`微信OCR`、词典部分软件内置了`必应`、`金山`词典，上述离线数据包配置较为繁琐，可直接使用软件内置服务