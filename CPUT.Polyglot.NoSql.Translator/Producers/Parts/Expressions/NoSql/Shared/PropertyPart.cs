using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Models.Mapper;
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

        internal string MemberOf { get; set; }

        internal string MemberOfType { get; set; }

        internal string MemberOfAlias { get; set; }

        internal bool IsKey { get; set; }

        public PropertyPart(Properties properties, Link link, BaseExpr baseExpr)
        {
            Name = link.Property;

            if(baseExpr is PropertyExpr)
            {
                var property = (PropertyExpr)baseExpr;

                AliasIdentifier = property.AliasIdentifier;
                AliasName = property.AliasName;
            }
            else if (baseExpr is TermExpr)
            {
                var term = (TermExpr)baseExpr;

                AliasIdentifier = term.AliasIdentifier;
                AliasName = term.AliasName;
            }

            if(string.IsNullOrEmpty(AliasIdentifier))
                AliasIdentifier = link.Reference.Substring(0, 3).ToLower();

            if (!string.IsNullOrEmpty(link.Reference_Property))
            {
                MemberOf = link.Reference_Property;
                MemberOfType = link.Reference_Type;
                MemberOfAlias = link.Reference_Property.Substring(0, 2).ToLower();
            }
            
            IsKey = properties.Key;
        }

        public PropertyPart(BaseExpr expr)
        {
            if(expr is StringLiteralExpr)
            {
                Name = ((StringLiteralExpr)expr).Value;
                Type = "string";
            }
            else if (expr is NumberLiteralExpr)
            {
                Name = ((NumberLiteralExpr)expr).Value.ToString();
                Type = "number";
            }
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
