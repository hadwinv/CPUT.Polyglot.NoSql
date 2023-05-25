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

        internal string ParentReference { get; set; }


        public UnwindGraphPart(PropertyExpr expr, Model parent, Model child)
        {
            UnwindProperty = child.Name;

            if (!string.IsNullOrEmpty(expr.AliasIdentifier))
                ParentReferenceAlias = expr.AliasIdentifier;
            else
                ParentReferenceAlias = parent.Name.Substring(0, 3).ToLower();

            UnwindedAlias = child.Name.Substring(0, 3).ToLower();

            ParentReference = parent.Name;
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
