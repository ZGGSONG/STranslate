using System.Windows;

namespace STranslate
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static System.Threading.Mutex mutex;

        //系统能够识别有名称的互斥，因此可以使用它禁止应用程序启动两次
        //第二个参数可以设置为产品的名称:Application.ProductName
        // 每次启动应用程序，都会验证名称为OnlyRun的互斥是否存在
        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new System.Threading.Mutex(true, "CE252DD8-179F-4544-9989-453F5DEA378D");
            if (mutex.WaitOne(0, false))
            {
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show("STranslate 已在运行...", "提示");
                this.Shutdown();
            }
        }
    }
}