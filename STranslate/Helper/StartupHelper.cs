using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IWshRuntimeLibrary;

namespace STranslate.Helper
{
    public class StartupHelper
    {

        #region public method
        /// <summary>
        /// 设置开机自启
        /// </summary>
        public static void SetStartup()
        {
            ShortCutCreate();
        }
        /// <summary>
        /// 检查是否已经设置开机自启
        /// </summary>
        /// <returns>true: 开机自启 false: 非开机自启</returns>
        public static bool IsStartup()
        {
            return ShortCutExist(appPath, StartUpPath);
        }
        /// <summary>
        /// 取消开机自启
        /// </summary>
        public static void UnSetStartup()
        {
            ShortCutDelete(appPath, StartUpPath);
        }
        #endregion

        #region params
        /// <summary>
        /// 开机启动目录
        /// </summary>
        private static readonly string StartUpPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        /// <summary>
        /// 当前程序二进制文件路径
        /// </summary>
        private static readonly string appPath = Assembly.GetEntryAssembly().Location;

        /// <summary>
        /// 组合的开机启动目录中的快捷方式路径
        /// </summary>
        private static readonly string appShortcutPath = Path.Combine(StartUpPath, Path.GetFileNameWithoutExtension(appPath) + ".lnk");
        #endregion

        #region native method
        /// <summary>
        /// 获取快捷方式中的目标（可执行文件的绝对路径）
        /// </summary>
        /// <param name="shortCutPath">快捷方式的绝对路径</param>
        /// <returns></returns>
        /// <remarks>需引入 COM 组件 Windows Script Host Object Model</remarks>
        private static string GetAppPathViaShortCut(string shortCutPath)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortct = (IWshShortcut)shell.CreateShortcut(shortCutPath);
                //快捷方式文件指向的路径.Text = 当前快捷方式文件IWshShortcut类.TargetPath;
                //快捷方式文件指向的目标目录.Text = 当前快捷方式文件IWshShortcut类.WorkingDirectory;
                return shortct.TargetPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取指定文件夹下的所有快捷方式（不包括子文件夹）
        /// </summary>
        /// <param target="">目标文件夹（绝对路径）</param>
        /// <returns></returns>
        private static List<string> GetDirectoryFileList(string target)
        {
            List<string> list = new List<string>();
            list.Clear();
            string[] files = Directory.GetFiles(target, "*.lnk");
            if (files == null || files.Length == 0)
            {
                return list;
            }
            for (int i = 0; i < files.Length; i++)
            {
                list.Add(files[i]);
            }
            return list;
        }

        /// <summary>
        /// 判断快捷方式是否存在
        /// </summary>
        /// <param name="path">快捷方式目标（可执行文件的绝对路径）</param>
        /// <param target="">目标文件夹（绝对路径）</param>
        /// <returns></returns>
        private static bool ShortCutExist(string path, string target)
        {
            bool Result = false;
            List<string> list = GetDirectoryFileList(target);
            foreach (var item in list)
            {
                if (path == GetAppPathViaShortCut(item))
                {
                    Result = true;
                }
            }
            return Result;
        }

        /// <summary>
        /// 删除快捷方式（通过快捷方式目标进行删除）
        /// </summary>
        /// <param name="path">快捷方式目标（可执行文件的绝对路径）</param>
        /// <param target="">目标文件夹（绝对路径）</param>
        /// <returns></returns>
        private static bool ShortCutDelete(string path, string target)
        {
            bool Result = false;
            List<string> list = GetDirectoryFileList(target);
            foreach (var item in list)
            {
                if (path == GetAppPathViaShortCut(item))
                {
                    System.IO.File.Delete(item);
                    Result = true;
                }
            }
            return Result;
        }
        /// <summary>
        /// 为本程序创建一个开机启动快捷方式
        /// </summary>
        private static bool ShortCutCreate()
        {
            bool Result = false;
            try
            {
                ShortCutDelete(appPath, StartUpPath);

                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                var shortcut = shell.CreateShortcut(appShortcutPath);
                shortcut.TargetPath = Assembly.GetEntryAssembly().Location;
                shortcut.Arguments = string.Empty;
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                shortcut.Save();
                Result = true;
            }
            catch
            {
                Result = false;
            }
            return Result;
        }
        #endregion
    }
}
