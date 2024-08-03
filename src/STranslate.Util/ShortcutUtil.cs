using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using STranslate.Model;

namespace STranslate.Util;

public class ShortcutUtil
{
    #region Public Method

    /// <summary>
    ///     设置开机自启
    /// </summary>
    public static void SetStartup()
    {
        ShortCutCreate();
    }

    /// <summary>
    ///     检查是否已经设置开机自启
    /// </summary>
    /// <returns>true: 开机自启 false: 非开机自启</returns>
    public static bool IsStartup()
    {
        return ShortCutExist(AppPath, StartUpPath);
    }

    /// <summary>
    ///     取消开机自启
    /// </summary>
    public static void UnSetStartup()
    {
        ShortCutDelete(AppPath, StartUpPath);
    }

    /// <summary>
    ///     设置桌面快捷方式
    /// </summary>
    public static void SetDesktopShortcut()
    {
        ShortCutCreate(true);
    }

    #endregion

    #region Param

    /// <summary>
    ///     开机启动目录
    /// </summary>
    private static readonly string StartUpPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

    /// <summary>
    ///     用户桌面目录
    /// </summary>
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    /// <summary>
    ///     当前程序二进制文件路径
    /// </summary>
    private static readonly string AppPath = $"{ConstStr.ExecutePath}{ConstStr.AppName}.exe";

    /// <summary>
    ///     组合的开机启动目录中的快捷方式路径
    /// </summary>
    private static readonly string AppShortcutPath =
        Path.Combine(StartUpPath, Path.GetFileNameWithoutExtension(AppPath) + ".lnk");

    private static readonly string DesktopShortcutPath =
        Path.Combine(DesktopPath, Path.GetFileNameWithoutExtension(AppPath) + ".lnk");

    #endregion

    #region Private Method

    /// <summary>
    ///     获取指定文件夹下的所有快捷方式（不包括子文件夹）
    /// </summary>
    /// <param name="target">目标文件夹（绝对路径）</param>
    /// <returns></returns>
    private static List<string> GetDirectoryFileList(string target)
    {
        List<string> list = [];
        list.Clear();
        var files = Directory.GetFiles(target, "*.lnk");
        if (files.Length == 0) return list;

        list.AddRange(files);
        return list;
    }

    /// <summary>
    ///     判断快捷方式是否存在
    /// </summary>
    /// <param name="path">快捷方式目标（可执行文件的绝对路径）</param>
    /// <param name="target">目标文件夹（绝对路径）</param>
    /// <returns></returns>
    private static bool ShortCutExist(string path, string target)
    {
        var result = false;
        var list = GetDirectoryFileList(target);
        foreach (var item in list.Where(item => path == GetAppPathViaShortCut(item))) result = true;
        return result;
    }

    /// <summary>
    ///     删除快捷方式（通过快捷方式目标进行删除）
    /// </summary>
    /// <param name="path">快捷方式目标（可执行文件的绝对路径）</param>
    /// <param name="target">目标文件夹（绝对路径）</param>
    /// <returns></returns>
    private static bool ShortCutDelete(string path, string target)
    {
        var result = false;
        var list = GetDirectoryFileList(target);
        foreach (var item in list.Where(item => path == GetAppPathViaShortCut(item)))
        {
            File.Delete(item);
            result = true;
        }

        return result;
    }

    /// <summary>
    ///     为本程序创建一个快捷方式
    /// </summary>
    /// <param name="isDesktop">是否为桌面快捷方式</param>
    /// <returns></returns>
    private static bool ShortCutCreate(bool isDesktop = false)
    {
        var result = true;
        try
        {
            CreateShortcut(isDesktop ? DesktopShortcutPath : AppShortcutPath, AppPath, AppPath);
        }
        catch
        {
            result = false;
        }

        return result;
    }

    #region 非 COM 实现快捷键创建

    /// <see href="https://blog.csdn.net/weixin_42288222/article/details/124150046" />
    /// <summary>
    ///     获取快捷方式中的目标（可执行文件的绝对路径）
    /// </summary>
    /// <param name="shortCutPath">快捷方式的绝对路径</param>
    /// <returns></returns>
    private static string? GetAppPathViaShortCut(string shortCutPath)
    {
        try
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var file = (IShellLink)new ShellLink();
            file.Load(shortCutPath, 2);
            var sb = new StringBuilder(256);
            file.GetPath(sb, sb.Capacity, IntPtr.Zero, 2);
            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     向目标路径创建指定文件的快捷方式
    /// </summary>
    /// <param name="shortcutPath">快捷方式路径</param>
    /// <param name="appPath">App路径</param>
    /// <param name="description">提示信息</param>
    private static void CreateShortcut(string shortcutPath, string appPath, string description)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        var link = (IShellLink)new ShellLink();
        //link.SetDescription(description);
        link.SetPath(appPath);

        if (File.Exists(shortcutPath))
            File.Delete(shortcutPath);
        link.Save(shortcutPath, false);
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLink : IPersistFile
    {
        void GetPath([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd,
            int fFlags);

        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);

        void GetIconLocation([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath,
            out int piIcon);

        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    #endregion

    #endregion
}