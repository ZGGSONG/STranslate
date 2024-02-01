using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Util
{
    public class Volcengine
    {
        [DllImport("volcengine.dll", EntryPoint = "Execute", CallingConvention = CallingConvention.Cdecl)]
        public static extern GoTuple Execute(byte[] appid, byte[] appkey, byte[] source, byte[] target, byte[] content);

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

        public static Tuple<int, string> GoTupleToCSharpTuple(GoTuple tuple)
        {
            var intvalue = tuple.intValue;
            var stringvalue = GoStringToCSharpString(tuple.stringValue);
            return Tuple.Create(intvalue, stringvalue);
        }

        public static string GoStringToCSharpString(GoString goString)
        {
            byte[] bytes = new byte[goString.n];
            for (int i = 0; i < goString.n; i++)
            {
                bytes[i] = Marshal.ReadByte(goString.p, i);
            }
            string result = Encoding.UTF8.GetString(bytes);
            return result;
        }
    }
}
