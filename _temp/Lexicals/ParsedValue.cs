using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public struct ParsedValue<T>
    {
        public ParsedValue(T value, TextSpan span)
        {
            Value = value;
            Span = span;
        }

        public T Value { get; }
        public bool HasSpan => Span != TextSpan.None;
        public TextSpan Span { get; }
    }
}
