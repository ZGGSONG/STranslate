
WPF 开发的一款即开即用、即用即走的翻译工具

## 功能

- [x] 添加 DeepL API
- [x] 添加百度翻译 API
- [x] 实现基本翻译功能
- [x] 优化防止多开
- [x] 添加划词翻译
- [x] 添加复制结果蛇形、大小驼峰
- [x] 优化 DeepL 服务并打包成库引入进项目
> 自己构造的接口测试下来还是不够稳定，本来也是启发于 [ZU1k](https://github.com/zu1k) 就先用他开发的接口吧
- [ ] 添加 OCR 翻译
- [ ] 优化划词翻译（未选中则取消翻译）
- [ ] 添加软件缓存功能
- [ ] 添加翻译记录缓存功能
- [ ] 设置 UI 化
- [ ] 使用说明 UI 化
- [ ] 软件层面识别语种（UI 提示识别语种）
- [ ] 翻译制作成插件方式
- [ ] 优化软件发布方式(自动升级)

## 使用

打开软件后会静默在后台，等待调用
1. 全局监听快捷键
- `Alt` + `A` 打开软件界面，输入内容按回车翻译
- `Alt` + `D` 复制当前鼠标选中内容并翻译

2. 软件内快捷键
- `ESC` 隐藏界面
- `Ctrl+Shift+Q` 退出程序
- `Ctrl+Shift+T` 置顶/取消置顶

点击软件外部界面任意处或点击软件则会自动隐藏到后台，即用即走。

## 参考项目

监听全局快捷键功能参考如下项目，在此表示感谢
- [https://github.com/NPCDW/WpfTool](https://github.com/NPCDW/WpfTool)
翻译接口使用项目 [ZU1K](https://github.com/zu1k/removed-2022-07-12/releases/tag/0.1.2)
- [https://github.com/zu1k/removed-2022-07-12/releases/tag/0.1.2](https://github.com/zu1k/removed-2022-07-12/releases/tag/0.1.2)