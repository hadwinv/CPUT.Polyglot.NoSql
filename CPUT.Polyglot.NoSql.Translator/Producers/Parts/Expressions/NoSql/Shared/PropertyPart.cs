using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared
{
    public class PropertyPart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal string AliasName { get; set; }

        internal string Type { get; set; }

        public PropertyPart(Link link, BaseExpr baseExpr, int target)
        {
            Name = link.Property;

            dynamic? expr = baseExpr is PropertyExpr ? ((PropertyExpr)baseExpr) :
                            baseExpr is TermExpr ? ((TermExpr)baseExpr) :
                            baseExpr is JsonExpr ? ((JsonExpr)baseExpr) : default;

            if(expr != null)
            {
                AliasIdentifier = expr.AliasIdentifier;
                AliasName = expr.AliasName;

                if (expr is JsonExpr)
                {
                    var child = Assistor.NSchema[target].SelectMany(x => x.Model.Where(x => x.Name == link.Reference)).FirstOrDefault();

                    if (child != null)
                        Name = Assistor.UnwindPropertyName(child, target) + "." + link.Property;
                }
            }

            if (string.IsNullOrEmpty(AliasIdentifier))
                AliasIdentifier = link.Reference.Substring(0, 3).ToLower();
        }

        public PropertyPart(BaseExpr baseExpr)
        {
            dynamic? expr = baseExpr is StringLiteralExpr ? ((StringLiteralExpr)baseExpr) : ((NumberLiteralExpr)baseExpr);

            Name = expr is StringLiteralExpr ? expr.Value : expr.Value.ToString();
            Type = expr is StringLiteralExpr ? "string" : "int";
        }

        public void Accept(INeo4jVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(ICassandraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(IRedisVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
