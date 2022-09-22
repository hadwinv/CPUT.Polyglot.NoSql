using Superpower.Model;

namespace CPUT.Polyglot.NoSql.Common.Helpers
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
