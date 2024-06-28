using System;
using WindowsInput;

namespace STranslate.Helper;

public class InputSimulatHelper
{
    private static readonly InputSimulator InputSimulator = new();

    public static void PrintText(object? obj)
    {
        if (obj == null) return;

        PrintText(obj.ToString());
    }

    public static void PrintText(string? content)
    {
        if (string.IsNullOrEmpty(content)) return;

        InputSimulator.Keyboard.TextEntry(content);
    }

    public static void Backspace(int count = 1)
    {
        for (int i = 0; i < count; i++)
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.BACK);
    }
}