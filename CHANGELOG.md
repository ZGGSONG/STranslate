# 注意

1. 该版本修复不少人反馈的OCR报错数据包不存在的问题, 该问题系用户使用批处理命令启动软件, 导致函数`Environment.CurrentDirectory` 获取程序路径出错导致, 现已修复 感谢@qingmeng006在#149中指出
2. 该版本更新`PaddleOCRSharp`到`4.3`版本，减小依赖库体积，实测效率有小幅提升，请进行完整更新(手动下载完整安装包解压后使用)

> 请以后提`Bug Issue`时提供完整复现条件, 否则一律不予修复

## Updates

- Added: Support for Volcengine OCR
- Added: Optional modification of `Screenshot Translation`/`Silent OCR` language (with a quick modification entry on the main interface)
- Added: Configuration of commonly used languages #139
- Added: Quick access to configuration and log viewing in the about interface
- Added: Log size viewing and one-click log clearing function
- Added: Rewrote Screenshot Library [ScreenGrab](https://github.com/ZGGSONG/ScreenGrab) and replaced it, optimizing the user experience
- Added: Mouse selection incremental translation & added software internal hotkey `Ctrl`+`E` to enable/disable incremental translation #137
- Added: Software internal shortcut key `Ctrl`+`Shift`+`M` to return to the middle of the main screen
- Added: Optional opening of the software at the last closed position
- Added: Back translation function and optional display of function buttons
- Optimized: Updated PaddleOCRSharp version to 4.3
- Optimized: Optional closing of hotkey trigger copy reminders
- Optimized: Full-size icon #119 Thanks to @ema
- Optimized: Password box and watermark box styles
- Optimized: Optimized OCR, TTS added service default not enabled
- Optimized: Gemini supports model selection #126
- Optimized: Improved translation replacement experience
- Optimized: Updated Ollama default API address
- Optimized: When opening the software, if it is too close to the screen edge, it will automatically return to the middle of the screen
- Optimized: Optimized OCR, network error, and cancel operation log printing to avoid ambiguity
- Optimized: Hide copy and insert buttons when output interface is cancelled or network error occurs
- Fixed: Logical conflict between insert text and top status
- Fixed: Replacement translation import not taking effect
- Fixed: Output result cannot be selected by mouse sliding
- Fixed: OCR prompt no data package problem after batch processing startup #149 Thanks to @qingmeng006


**Full Update Log:** [1.1.6.704...1.1.7.808](https://github.com/ZGGSONG/STranslate/compare/1.1.6.704...1.1.7.808)

## Cloud Disk Download

[Lanzouyun](https://zggsong.lanzoub.com/b02qrag7sd) | [123 Pan](https://www.123pan.com/s/AxlRjv-OuVmA.html)

lanzouyun password: 
```txt
song
```

---

## 更新

- 新增: 添加火山OCR支持
- 新增: 添加可选修改`截图翻译`/`静默OCR`语种修改(可选主界面快捷修改入口)
- 新增: 添加常用语种配置 #139
- 新增: 关于界面查看配置及日志快捷入口
- 新增: 日志大小查看及一键清理日志功能
- 新增: 重写截图库[ScreenGrab](https://github.com/ZGGSONG/ScreenGrab)并替换,优化使用体验
- 新增: 添加鼠标划词增量翻译&添加软件内热键`Ctrl`+`E`启用/禁用增量翻译 #137
- 新增: 软件内快捷键`Ctrl`+`Shift`+`M`回到主屏幕中间
- 新增: 添加可选打开软件时是否使用上次关闭前的位置
- 新增: 添加回译功能并可选开启显示功能按钮
- 优化: 更新PaddleOCRSharp版本到4.3
- 优化: 可选关闭热键触发复制提醒
- 优化: 全尺寸icon #119 感谢@ema
- 优化: 密码框、水印框样式
- 优化: 优化OCR,TTS添加服务时默认不启用
- 优化: Gemini支持模型自选 #126
- 优化: 提升替换翻译体验
- 优化: 更新Ollama默认api地址
- 优化: 打开软件时过于贴近屏幕边缘则自动回到屏幕中间
- 优化: 优化OCR、网络错误、取消操作日志打印, 避免歧义
- 优化: 输出界面取消或网络错误问题时隐藏复制插入等按钮
- 修复: 插入文本和置顶状态之间的逻辑冲突问题
- 修复: 替换翻译导入不生效的问题
- 修复: 输出结果无法鼠标滑动选中文本的的问题
- 修复: 批处理启动软件后ocr提示没有数据包的问题 #149 感谢@qingmeng006

**完整更新日志:** [1.1.6.704...1.1.7.808](https://github.com/ZGGSONG/STranslate/compare/1.1.6.704...1.1.7.808)

## 网盘下载

[蓝奏云](https://zggsong.lanzoub.com/b02qrag7sd) | [123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

蓝奏云密码: 
```txt
song
```

## 感谢

@ema
@qingmeng006
