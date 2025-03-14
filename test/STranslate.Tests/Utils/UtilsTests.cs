using STranslate.Util;
using Xunit;

namespace STranslate.Tests.Utils;

public class UtilsTests
{
    public static string DocumentToBase64Str(string fileName)
    {
        using FileStream filestream = new(fileName, FileMode.Open);

        var bt = new byte[filestream.Length];
        //调用read读取方法
        filestream.Read(bt, 0, bt.Length);
        var base64Str = Convert.ToBase64String(bt);
        return base64Str;
    }
}