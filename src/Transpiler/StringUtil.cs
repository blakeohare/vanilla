using System;
using System.Collections.Generic;
using System.Text;

namespace Vanilla.Transpiler
{
    internal static class StringUtil
    {
        public static string stringValueToCode(string value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('"');
            char c;
            for (int i = 0; i < value.Length; i++)
            {
                c = value[i];
                switch (c)
                {
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\0': sb.Append("\\0"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\'': sb.Append("\\'"); break;
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append("\\\\"); break;
                    default:
                        sb.Append(c);
                        break;
                }
             }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
