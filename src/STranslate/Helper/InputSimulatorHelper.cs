using STranslate.Util;
using WindowsInput;

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

    /// <summary>
    ///     使用粘贴输出
    ///     * 避免部分用户无法正常输出的问题
    /// </summary>
    /// <param name="content"></param>
    public static void PrintTextWithClipboard(string content)
    {
        // 检查内容是否为空或仅包含空白字符
        if (string.IsNullOrEmpty(content)) return;
        ClipboardHelper.Copy(content);
        ClipboardUtil.SendCtrlCV(false);
    }

    public static void Backspace(int count = 1)
    {
        for (var i = 0; i < count; i++)
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.BACK);
    }
}