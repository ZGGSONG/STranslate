using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Model
{
    public static class HotKeys
    {
        public static class InputTranslate
        {
            public static byte Modifiers = 1;
            public static int Key = 65;
            public static String Text = "A";
            public static bool Conflict = false;
        }
        public static class CrosswordTranslate
        {
            public static byte Modifiers = 0;
            public static int Key = 113;
            public static String Text = "F2";
            public static bool Conflict = false;
        }
        public static class ScreenShotTranslate
        {
            public static byte Modifiers = 1;
            public static int Key = 83;
            public static String Text = "S";
            public static bool Conflict = false;
        }
    }
}
