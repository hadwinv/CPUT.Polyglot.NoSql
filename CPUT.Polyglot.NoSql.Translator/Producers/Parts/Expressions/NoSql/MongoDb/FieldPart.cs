using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class FieldPart : IExpression
    {
        internal IExpression[] Fields { get; set; }

        public FieldPart()
        {
        }

        public FieldPart(IExpression[] fields)
        {
            Fields = fields;
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
