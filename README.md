<p align="center">
<a href="https://github.com/zggsong/STranslate" target="_blank">
<img align="center" alt="STranslate" width="200" src="./translate.svg" />
</a>
</p>
<p align="center">
<a href="https://github.com/ZGGSONG/STranslate/blob/main/LICENSE" target="_self">
 <img alt="Latest GitHub release" src="https://img.shields.io/github/license/ZGGSONG/STranslate" />
</a>
<a href="https://github.com/ZGGSONG/STranslate/releases/latest" target="_blank">
 <img alt="Latest GitHub release" src="https://img.shields.io/github/release/ZGGSONG/STranslate.svg" />
</a>
<a href="https://hub.docker.com/r/zggsong/translate">
  <img alt="Docker pull" src="https://img.shields.io/docker/pulls/zggsong/translate">
</a>
<a href="https://github.com/ZGGSONG/STranslate" target="_self">
 <img alt="GitHub last commit" src="https://img.shields.io/github/last-commit/ZGGSONG/STranslate" />
</a>
</p>
<h1 align="center">STranslate</h1>

<p align="center">WPF 开发的一款<strong>即开即用</strong>、<strong>即用即走</strong>的翻译工具
<br> 中文 | <a href="README_EN.md">English</a>
</p>




[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FZGGSONG%2FSTranslate.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FZGGSONG%2FSTranslate?ref=badge_large)

## 功能

- [x] 添加 DeepL API
- [x] 添加划词翻译
- [x] 添加复制结果蛇形、大小驼峰
- [x] 软件层面识别语种(中英文)
- [x] 添加开机自启
- [x] 添加明/暗主题
- [x] 添加 UI 设置缓存(用户目录下 `AppData\Local\STranslate`)
- [x] 添加离线语音合成
- [x] 添加离线截图文字识别(支持英文, 中文数据包过大且体验不好)
- [x] 添加检查更新
- [x] 添加翻译记录缓存功能

## 安装

下载最新 [Release](https://github.com/ZGGSONG/STranslate/releases) 版本后解压即可使用

## 使用

![previews](./example.png)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FZGGSONG%2FSTranslate.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FZGGSONG%2FSTranslate?ref=badge_shield)

![previews_dark](./example_dark.png)

打开软件后会静默在后台，等待调用

1. 全局监听快捷键
- `Alt` + `A` 打开软件界面，输入内容按回车翻译
- `Alt` + `D` 复制当前鼠标选中内容并翻译
- `Alt` + `S` 截图选中区域内容并翻译
- `Alt` + `G` 打开窗口(不清空内容)

2. 软件内快捷键
- `ESC` 隐藏界面
- `Ctrl+Shift+Q` 退出程序
- `Ctrl+Shift+R` 切换主题
- `Ctrl+Shift+T` 置顶/取消置顶

点击软件外部任意处即自动隐藏到后台——即用即走。


> STranslate依赖于.NET Framework 4.8 运行环境，如果程序启动时提示“This application requires *** .NETFramework,Version=v4.8”，请点击以下链接下载并安装.NET Framework 4.8 运行环境。  
> [适用于 Windows 的 Microsoft .NET Framework 4.8 脱机安装程序下载](https://download.visualstudio.microsoft.com/download/pr/2d6bb6b2-226a-4baa-bdec-798822606ff1/8494001c276a4b96804cde7829c04d7f/ndp48-x86-x64-allos-enu.exe) | [Microsoft Support](https://support.microsoft.com/zh-cn/topic/%E9%80%82%E7%94%A8%E4%BA%8E-windows-%E7%9A%84-microsoft-net-framework-4-8-%E8%84%B1%E6%9C%BA%E5%AE%89%E8%A3%85%E7%A8%8B%E5%BA%8F-9d23f658-3b97-68ab-d013-aa3c3e7495e0)

## 卸载

1. 删除软件运行目录
2. 打开 cmd 运行下面的命令即可

```shell
rd /s /q "%localappdata%\stranslate"
```

## 开发历史

- 2023-03-02 0.25 添加复制提醒动画

- 2023-02-28 0.24 添加 deepl 接口(已经安装的cmd运行 `del %localappdata%\stranslate\stranslate.json` 后打开即可更新接口)

- 2023-02-24 0.22 优化分辨率切换时托盘图标模糊问题

- 2023-01-17 0.20 添加翻译记录缓存功能，重复翻译从本地数据库获取，本地记录数量上限可调整

- 2023-01-12 0.18 优化 GC 后台静默运行内存占用保持 4MB 左右

- 2023-01-12 0.17 添加检查更新功能

- 2023-01-10 0.15 添加离线 OCR 功能，其使用 [tesseract](https://github.com/tesseract-ocr/tesseract) 目前仅支持英文

<details>
  <summary>自修改提示</summary>

有经验者可自行下载 [语言包](https://github.com/tesseract-ocr/tessdata) 至 `tessdata` 目录后修改 `Util`中`TesseractGetText`方法即可

```C#
public static string TesseractGetText(Bitmap bmp)
{
	try
	{
		using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
		//using (var engine = new TesseractEngine(@"./tessdata", "chi_sim", EngineMode.Default))
		{
			using(var pix = PixConverter.ToPix(bmp))
			{
				using (var page = engine.Process(pix))
				{
					return page.GetText();
				}
			}
		}
	}
	catch (Exception ex)
	{
		throw ex;
	}
}
```
</details>

- 2023-12-28 0.10 添加明暗主题切换功能

- 2022-12-27 0.08 版本添加开机启动

## 如果接口失效

当请求人数较多时，远端接口可能暂时失效，可自行运行翻译接口程序
1. **【推荐】** 下载对应平台可 [执行文件](https://github.com/ZGGSONG/STranslate/releases/tag/0.01)，随后在软件右上角选择 `local` 接口即可
2. 【进阶】 下载 [docker镜像](https://hub.docker.com/r/zggsong/translate)，关闭软件 - cmd 运行 `start %localappdata%\stranslate\stranslate.json` - 修改接口地址 - 重启软件即可

## Author 作者

**STranslate** © [zggsong](https://github.com/zggsong), Released under the [MIT](https://github.com/ZGGSONG/STranslate/blob/main/LICENSE) License.<br>
Authored and maintained by zggsong with help from other open source projects [WpfTool](https://github.com/NPCDW/WpfTool) and [Tai](https://github.com/Planshit/Tai).

> Website [Blog](https://www.zggsong.com) · GitHub [@zggsong](https://github.com/zggsong)