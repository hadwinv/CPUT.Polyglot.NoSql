using Superpower.Model;

namespace CPUT.Polyglot.NoSql.Interface.Parser
{
    public interface IParserElement
    {
        TextSpan? Span { get; }

        void Accept(IParserVisitor visitor);
    }
}
