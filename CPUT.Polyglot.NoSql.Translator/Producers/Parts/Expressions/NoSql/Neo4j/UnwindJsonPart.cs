using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j
{
    public class UnwindJsonPart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal string UnwindAliasIdentifier { get; set; }

        public UnwindJsonPart(Link link, PropertyExpr property)
        {
            Name = link.Reference_Property;

            if (!string.IsNullOrEmpty(property.AliasIdentifier))
                AliasIdentifier = property.AliasIdentifier;
            else
                AliasIdentifier = link.Reference.Substring(0, 3).ToLower();

            UnwindAliasIdentifier = link.Reference_Property.Substring(0, 2).ToLower();
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
