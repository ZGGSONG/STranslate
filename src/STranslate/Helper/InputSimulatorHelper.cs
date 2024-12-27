using System.Threading;
using System.Windows;
using WindowsInput;
using STranslate.Util;

namespace STranslate.Helper;

public class InputSimulatorHelper
{
    private static readonly InputSimulator InputSimulator = new();

    public static void PrintText(object? obj)
    {
        if (obj == null) return;

        PrintText(obj.ToString());
    }

    public static void PrintText(string? content)
    {
        // 检查内容是否为空或仅包含空白字符
        if (string.IsNullOrEmpty(content)) return;
        
        // 判断是否使用粘贴输出
        if (Singleton<ConfigHelper>.Instance.CurrentConfig?.UsePasteOutput ?? false)
        {
            
            IDataObject originalClipboard = Clipboard.GetDataObject();
            
            try
            {
                // 确保剪贴板操作完成
                Thread.Sleep(50);
                Clipboard.SetText(content);
                Thread.Sleep(50);

                InputSimulator simulator = new InputSimulator();
                simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                
                // 等待粘贴操作完成
                Thread.Sleep(100);
            }
            finally
            {
                if (originalClipboard != null)
                {
                    Thread.Sleep(50);
                    Clipboard.SetDataObject(originalClipboard, true);
                }
            }
        }
        else
        {
            // 分割字符串为多行
            var lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

            // 处理流式输出中单独的换行符号: \r\n  \r  \n  \n\n
            if (lines.All(x => x == ""))
            {
                // 一个换行会分割出两个空字符串，所以长度减一
                for (var i = 0; i < lines.Length - 1; i++) InputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);

                // 长度为1的情况为空字符串直接返回
                return;
            }

            foreach (var line in lines)
            {
                InputSimulator.Keyboard.TextEntry(line);
                // 模拟按下回车键，除了最后一行
                if (!line.Equals(lines.Last())) InputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            }
        }

    }

    public static void Backspace(int count = 1)
    {
        for (var i = 0; i < count; i++)
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.BACK);
    }
}