using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class GetCollectionPart : IExpression
    {
        internal string Name { get; set; }

        public GetCollectionPart(string name)
        {
            Name = name;
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
