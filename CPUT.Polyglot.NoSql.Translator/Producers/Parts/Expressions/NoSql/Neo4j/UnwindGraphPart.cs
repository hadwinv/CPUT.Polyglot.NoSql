using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j
{
    public class UnwindGraphPart : IExpression
    {
        internal string UnwindProperty { get; set; }

        internal string UnwindedAlias { get; set; }

        internal string ParentReferenceAlias { get; set; }

        public UnwindGraphPart(LinkedProperty mappedProperty)
        {
            if(mappedProperty.Link != null)
            {
                UnwindProperty = mappedProperty.Link.Property;
                ParentReferenceAlias = mappedProperty.Link.Reference.Substring(0, 3).ToLower();
                UnwindedAlias = UnwindProperty.Substring(0, 3).ToLower();
            }    
        }

        public void Accept(INeo4jVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(ICassandraVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IRedisVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

}
