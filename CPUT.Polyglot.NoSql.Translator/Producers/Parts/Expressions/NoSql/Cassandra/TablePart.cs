﻿using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Cassandra
{
    public class TablePart : IExpression
    {
        internal string Name { get; set; }

        internal string Alias { get; set; }

        internal string Source { get; set; }

        public TablePart(string name, string alias, string source)
        {
            Name = name;
            Alias = alias;
            Source = source;
        }

        public void Accept(IRedisVisitor visitor)
        {
            throw new NotImplementedException(); ;
        }

        public void Accept(INeo4jVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(ICassandraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}
