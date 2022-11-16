using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j
{
    public class UnwindPart : IExpression
    {
        internal IExpression[] Expressions { get; set; }

        public UnwindPart(IExpression[] expressions)
        {
            Expressions = expressions;
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
