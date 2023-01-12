using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Updater
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private System.Threading.Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //  阻止多开和用户主动启动
            if (e.Args.Length == 0 || IsRuned())
            {
                Shutdown();
            }

            Activated += (s, e2) =>
            {
                var mainWindow = App.Current.MainWindow;
                if (mainWindow.DataContext != null)
                {
                    var mainViewModel = mainWindow.DataContext as MainViewModel;
                    if (string.IsNullOrEmpty(mainViewModel.Version))
                    {
                        mainViewModel.Version = e.Args[0];
                    }
                }
            };
        }





        #region 获取当前程序是否已运行
        /// <summary>
        /// 获取当前程序是否已运行
        /// </summary>
        private bool IsRuned()
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name, out ret);
            if (!ret)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
