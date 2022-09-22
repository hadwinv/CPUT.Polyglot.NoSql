namespace CPUT.Polyglot.NoSql.Interface.Parser
{
    public interface IParserVisitor
    {
        void Visit(IParserElement element);
    }
}
