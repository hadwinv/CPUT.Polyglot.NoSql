using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class UnwindJsonPart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal string UnwindAliasIdentifier { get; set; }

        public UnwindJsonPart(Link link, JsonExpr expr, int target)
        {
            var child = Assistor.NSchema[target].SelectMany(x => x.Model.Where(x => x.Name == link.Reference)).FirstOrDefault();

            if (child != null)
                Name = Assistor.UnwindPropertyName(child, target);

            if (!string.IsNullOrEmpty(expr.AliasIdentifier))
                AliasIdentifier = expr.AliasIdentifier;
            else
                AliasIdentifier = link.Reference.Substring(0, 3).ToLower();

            UnwindAliasIdentifier = link.Reference.Substring(0, 2).ToLower();
        }

        public void Accept(INeo4jVisitor visitor)
        {
            throw new NotImplementedException();
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
            visitor.Visit(this);
        }
    }
}
