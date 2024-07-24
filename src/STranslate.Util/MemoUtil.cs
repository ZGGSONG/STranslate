using System.Diagnostics;
using System.Runtime.InteropServices;

namespace STranslate.Util;

public class MemoUtil
{
    [DllImport("kernel32.dll")]
    private static extern bool SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

    /// <summary>
    ///     释放占用内存并重新分配,将暂时不需要的内容放进虚拟内存
    ///     当应用程序重新激活时，会将虚拟内存的内容重新加载到内存。
    ///     不宜过度频繁的调用该方法，频繁调用会降低使使用性能。
    ///     可在Close、Hide、最小化页面时调用此方法，
    /// </summary>
    public static void FlushMemory()
    {
        GC.Collect();
        // GC还提供了WaitForPendingFinalizers方法。
        GC.WaitForPendingFinalizers();
        GC.Collect();
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }

    /// <summary>
    ///     GC回收，释放占用内存并重新分配
    /// </summary>
    /// <param name="virtualMemo">是否将不需要的内容放进虚拟内存</param>
    /// <param name="sleepSpan">定时</param>
    public static void CrackerOnlyGC(bool virtualMemo = false, int sleepSpan = 30)
    {
        new Thread(_ =>
        {
            while (true)
                try
                {
                    if (virtualMemo)
                    {
                        FlushMemory();
                    }
                    else
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(sleepSpan));
                }
                catch
                {
                    // ignored
                }
        })
        {
            IsBackground = true
        }.Start();
    }
}