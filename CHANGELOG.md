## Updates

- Added: Integrated Google OCR API
- Added: Added link to official website on about page
- Added: Quick access to service configuration on main interface
- Added: Added Temperature configuration item for LLM service
- Added: Added translation function for individual services
- Added: Added software hotkey viewing function
- Added: Added portable configuration function
> Create a `portable_config` directory in the root directory to enable and only read and write configuration from that directory  
> Affected files: `stranslate.json`(user configuration), `stranslate.db`(history database), `stardict.db`(ECDICT database)
- Optimized: Optimized the logic of manual forced translation after the translation cache result is empty, greatly improving the user experience
- Optimized: Removed prompt content and directly translated after translation error
- Optimized: Added security settings for Gemini service as BLOCK_NONE to avoid translation issues
- Optimized: Fixed the issue of the list not scrolling after selecting an item in the background update
- Optimized: Released memory manually after screenshot translation and silent OCR
- Fixed: Configuration error cannot be reinitialized
- Fixed: Failed to delete log
- Fixed: Output interface still failed to get cache after manual translation
- Fixed: Simulated keyboard output space as newline issue
- Fixed: The main interface is hidden at startup but still displayed in the taskbar issue

**Full Update Log:** [1.1.8.810...1.1.9.821](https://github.com/ZGGSONG/STranslate/compare/1.1.8.810...1.1.9.821)

## Cloud Disk Download

[Lanzouyun](https://zggsong.lanzoub.com/b02qrag7sd) | [123 Pan](https://www.123pan.com/s/AxlRjv-OuVmA.html)

lanzouyun password: 
```txt
song
```

---

## 更新

- 新增: 接入谷歌OCR接口
- 新增: 添加关于页面官网链接
- 新增: 主界面配置服务快捷入口
- 新增: LLM服务添加`Temperature`配置项
- 新增: 添加单个服务执行翻译功能
- 新增: 添加软件热键查看功能
- 新增: 添加便携配置功能
> 根目录添加`portable_config`目录即可开启并将只会从该目录读写配置  
> 影响文件: `stranslate.json`(用户配置)、`stranslate.db`(历史记录数据库)、`stardict.db`(简明英汉词典数据库)
- 优化: 优化翻译缓存结果为空后需要手动强制翻译的逻辑,极大提升使用体验
- 优化: 翻译出错后移除提示内容并直接进行翻译
- 优化: Gemini 服务添加安全设置配置为`BLOCK_NONE`, 避免出现无法翻译问题
- 优化: 优化后台更新选中项目后列表不滚动的问题
- 优化: 截图翻译和静默OCR后手动释放内存
- 修复: 配置出错无法重新初始化开启问题
- 修复: 删除日志失败
- 修复: 输出界面手动点击翻译后仍获取缓存失败问题
- 修复: 模拟键盘输出空格为换行的问题
- 修复: 开启时隐藏主界面会显示在多任务中的问题

**完整更新日志:** [1.1.8.810...1.1.9.821](https://github.com/ZGGSONG/STranslate/compare/1.1.8.810...1.1.9.821)

## 网盘下载

[蓝奏云](https://zggsong.lanzoub.com/b02qrag7sd) | [123 网盘](https://www.123pan.com/s/AxlRjv-OuVmA.html)

蓝奏云密码: 
```txt
song
```
