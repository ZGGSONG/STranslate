## 更新

- 修复: 软件根目录创建`任务计划程序`脚本未指定工作目录

> 软件根目录脚本 `install_startup.bat` 和 `uninstall_startup.bat` 用来创建和删除任务计划程序，主要是为了以管理员权限启动软件而不经过`UAC弹窗`确认（也可以创建普通权限启动）
> **该脚本与软件内设置`开机自启动`不兼容，脚本创建任务计划程序后建议关闭软件内`开机启动项`配置**

**完整更新日志:** [1.5.1.407...1.5.2.408](https://github.com/ZGGSONG/STranslate/compare/1.5.1.407...1.5.2.408)

## 合作推广

🛠️ **官方API合作伙伴**  

[DeerAPI](https://api.deerapi.com/register?aff=j5dj) - AI聚合平台，一键调用500+模型，7折特惠，最新GPT4o、Grok 3、Gemini 2.5pro全支持！

[点击注册](https://api.deerapi.com/register?aff=j5dj)享免费试用额度，也能支持软件长久发展


## 离线数据

[123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

> 离线数据包含: [简明英汉词典数据包](https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip)  [PaddleOCR数据包(v4.3)](https://github.com/ZGGSONG/STranslate/releases/download/0.01/stranslate_paddleocr_data_v4.3.zip) 等

> 另: OCR部分软件内置`微信OCR`(1.5.0.402版本开始内置微信OCR数据包，无需依赖外部微信)、词典部分软件内置了`必应`、`金山`词典，上述离线数据包配置较为繁琐，可直接使用软件内置服务