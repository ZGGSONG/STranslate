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

<p align="center">
A <strong>ready-to-use, ready-to-go</strong> translation tool developed by WPF
<br> <a href="README.md">中文</a> | English
</p>



## Function

- [x] Add DeepL API
- [x] Add Crossword translation
- [x] Add replication results serpentine, large and small humps
- [x] Software level language recognition (Chinese and English)
- [x] Add Boot Self Start
- [x] Add a light/dark theme
- [x] Add UI settings cache (`AppData\Local\STranslate` in user directory)
- [x] Add offline voice synthesis
- [x] Add offline screenshot text recognition (supports English, Chinese data packets are too large and the experience is not good)
- [x] Add Check for Updates
- [x] Add translation record cache

## Install

Download the latest [Release](https://github.com/ZGGSONG/STranslate/releases) version and unzip it to use

## Useage

![previews](./example.png)

![previews_dark](./example_dark.png)

After opening the software it will be silent in the background, waiting for the call

1. Global Listening Shortcuts
- `Alt` + `A` Open the software interface, enter the content and press enter to translate
- `Alt` + `D` Copy the current mouse selection and translate it
- `Alt` + `S` Screenshot the content of the selected area and translate it
- `Alt` + `G` Open window (without emptying the contents)

2. In-software shortcuts
- `ESC` Hide interface
- `Ctrl+Shift+Q` Exit program
- `Ctrl+Shift+R` Switch Theme
- `Ctrl+Shift+T` Top/Cancel Top

Click anywhere outside of the software to automatically hide it in the background 


> NET Framework 4.8 runtime environment, if the application starts with the message "This application requires *** .NETFramework,Version=v4.8", please click on the following link NET Framework 4.8 Runtime Environment.  
> [NET Framework 4.8 for Windows Offline Installer Download](https://download.visualstudio.microsoft.com/download/pr/2d6bb6b2-226a-4baa-bdec-798822606ff1/8494001c276a4b96804cde7829c04d7f/ndp48-x86-x64-allos-enu.exe) | [Microsoft Support](https://support.microsoft.com/zh-cn/topic/%E9%80%82%E7%94%A8%E4%BA%8E-windows-%E7%9A%84-microsoft-net-framework-4-8-%E8%84%B1%E6%9C%BA%E5%AE%89%E8%A3%85%E7%A8%8B%E5%BA%8F-9d23f658-3b97-68ab-d013-aa3c3e7495e0)

## Uninstall

1. Delete the software run directory
2. Just open cmd and run the following command

```shell
rd /s /q "%localappdata%\stranslate"
```

## Development History

- 2023-03-02 0.25 Add copy alert animation

- 2023-02-28 0.24 Add Deepl interface(If you have already installed cmd, run `del %localappdata%\stranslate\stranslate.json ` and open it to update the interface)

- 2023-02-24 0.22 Optimize the problem of blurred tray icon when resolution switching

- 2023-01-17 0.20 Add translation record caching function, repeat translations are obtained from local database, and the upper limit of local records can be adjusted

- 2023-01-12 0.18 Optimized GC background silent running memory footprint remains around 4MB

- 2023-01-12 0.17 Add check update function

- 2023-01-10 0.15 Add offline OCR functionality, which uses [tesseract](https://github.com/tesseract-ocr/tesseract) currently only supports English

<details>
  <summary>Self-modification tips</summary>
If you are experienced, you can download the [language package](https://github.com/tesseract-ocr/tessdata) to the tessdata directory and modify the TesseractGetText method in the Util.

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

- 2023-12-28 0.10 Add light and dark theme switching function

- 2022-12-27 0.08 Versions add boot up

## If the interface fails

When the number of requests is large, the remote interface may temporarily fail, so you can run the translation interface program yourself
1. **【Recommend】** Download the [executable file](https://github.com/ZGGSONG/STranslate/releases/tag/0.01) for the corresponding platform and then select the local interface in the upper right corner of the software.
2. **【Advanced】** Download the [docker image](https://hub.docker.com/r/zggsong/translate), close the software - cmd run `start %localappdata%\stranslate\stranslate.json` - change the interface address - restart the software

## Author

**STranslate** © [zggsong](https://github.com/zggsong), Released under the [MIT](https://github.com/ZGGSONG/STranslate/blob/main/LICENSE) License.<br>
Authored and maintained by zggsong with help from other open source projects [WpfTool](https://github.com/NPCDW/WpfTool) and [Tai](https://github.com/Planshit/Tai).

> Website [Blog](https://www.zggsong.com) · GitHub [@zggsong](https://github.com/zggsong)
