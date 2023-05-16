using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared
{
    public class UnwindJsonPart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal string UnwindAliasIdentifier { get; set; }

        public UnwindJsonPart(Link link, JsonExpr expr)
        {
            var child = Assistor.NSchema.SelectMany(x => x.Model.Where(x => x.Name == link.Reference)).FirstOrDefault();

            if (child != null)
                Name = Assistor.UnwindPropertyName(child);

            if (!string.IsNullOrEmpty(expr.AliasIdentifier))
                AliasIdentifier = expr.AliasIdentifier;
            else
                AliasIdentifier = link.Reference.Substring(0, 3).ToLower();

            UnwindAliasIdentifier = link.Reference.Substring(0, 2).ToLower();
        }

        public UnwindJsonPart(PropertyExpr expr, Model model, string alias)
        {
            Name = alias;//expr.Value;

            if (!string.IsNullOrEmpty(expr.AliasIdentifier))
                AliasIdentifier = expr.AliasIdentifier;
            else
                AliasIdentifier = model.Name.Substring(0, 3).ToLower();

            UnwindAliasIdentifier = alias.Substring(0, 3).ToLower();
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
            visitor.Visit(this);
        }
    }
}
