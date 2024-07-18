using System.Runtime.InteropServices;
using System.Text;

namespace STranslate.Util;

public class GoUtil
{
    [DllImport("volcengine.dll", EntryPoint = "VolcengineTranslator", CallingConvention = CallingConvention.Cdecl)]
    public static extern GoTuple VolcengineTranslator(byte[] appid, byte[] appkey, byte[] source, byte[] target,
        byte[] content);

    [DllImport("volcengine.dll", EntryPoint = "VolcengineOcr", CallingConvention = CallingConvention.Cdecl)]
    public static extern GoTuple VolcengineOcr(byte[] appid, byte[] appkey, byte[] base64Str, byte[] action);

    [DllImport("volcengine.dll", EntryPoint = "TestMultiReturn", CallingConvention = CallingConvention.Cdecl)]
    public static extern GoTuple TestMultiReturn();

    public static Tuple<int, string> GoTupleToCSharpTuple(GoTuple tuple)
    {
        var intvalue = tuple.intValue;
        var stringvalue = GoStringToCSharpString(tuple.stringValue);
        return Tuple.Create(intvalue, stringvalue);
    }

    public static string GoStringToCSharpString(GoString goString)
    {
        var bytes = new byte[goString.n];
        for (var i = 0; i < goString.n; i++) bytes[i] = Marshal.ReadByte(goString.p, i);
        var result = Encoding.UTF8.GetString(bytes);
        return result;
    }

    public struct GoTuple
    {
        public int intValue;
        public GoString stringValue;
    }

    public struct GoString
    {
        public IntPtr p;
        public int n;

        public GoString(IntPtr n1, int n2)
        {
            p = n1;
            n = n2;
        }
    }
}