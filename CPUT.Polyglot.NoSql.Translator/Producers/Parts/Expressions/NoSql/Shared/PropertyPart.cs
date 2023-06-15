using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared
{
    public class PropertyPart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal string AliasName { get; set; }

        internal string Type { get; set; }

        public PropertyPart(LinkedProperty mappedProperty)
        {
            if(mappedProperty != null)
            {
                if (mappedProperty.Link.Target == Enum.GetName(typeof(Database), Database.NEO4J).ToLower())
                {
                    if (mappedProperty.Link.Property.IndexOf(".") > -1)
                    {
                        Name = mappedProperty.Link.Property.Split(".")[mappedProperty.Link.Property.Split(".").Count() - 1];
                        AliasIdentifier = mappedProperty.Link.Property.Split(".")[0].Substring(0, 3).ToLower();
                    }
                    else
                        Name = mappedProperty.Link.Property;
                }
                else
                {
                    Name = mappedProperty.Link.Property;
                    AliasIdentifier = mappedProperty.AliasIdentifier;

                    AliasName = mappedProperty.Link.Property.IndexOf(".") > -1
                        ? mappedProperty.Link.Property.Split(".")[mappedProperty.Link.Property.Split(".").Count() - 1] : mappedProperty.AliasName;
                }

                if (string.IsNullOrEmpty(AliasIdentifier))
                    AliasIdentifier = mappedProperty.Link.Reference.Substring(0, 3).ToLower();
            }
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
