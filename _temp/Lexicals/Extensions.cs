using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public static class Extensions
    {
        public static ParsedValue<T> ToParsedValue<T>(this T result, TextSpan start, TextSpan end)
        {
            return new ParsedValue<T>(result, start.UntilEnd(end));
        }

        public static ParsedValue<T> ToEmptyParsedValue<T>(this T result)
        {
            return new ParsedValue<T>(result, TextSpan.None);
        }

        public static TextSpan UntilEnd(this TextSpan @base, TextSpan? next)
        {
            if (next == null)
                return @base;

            int absolute1 = next.Value.Position.Absolute + next.Value.Length;
            int absolute2 = @base.Position.Absolute;
            return @base.First(absolute1 - absolute2);
        }

        public static ParsedValue<T> ToParsedValue<T>(this T result, TextSpan span)
        {
            return new ParsedValue<T>(result, span);
        }

        public static void PrintArray<T>(this ImmutableArray<T> arr, StringBuilder sb)
            where T : notnull
        {
            sb.Append("[ ");
            for (int i = 0; i < arr.Length; i++)
            {
                sb.Append(arr[i]);
                if (i < arr.Length - 1)
                    sb.Append(", ");
            }

            sb.Append(" ]");
        }

    }
}
