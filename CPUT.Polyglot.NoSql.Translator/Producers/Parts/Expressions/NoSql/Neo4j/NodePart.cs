﻿using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j
{
    public class NodePart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal List<RelationshipPart> Relations { get; set; }

        public NodePart(string name, string alias, Relations[] relations)
        {
            Name = name;

            if(!string.IsNullOrEmpty(alias))
                AliasIdentifier = alias.ToLower();
            else
                AliasIdentifier = name.Substring(0,3).ToLower();

            if (relations != null)
            {
                Relations = new List<RelationshipPart>();

                foreach (var relation in relations)
                    Relations.Add(new RelationshipPart(relation.Cardinality, relation.Reference));
            }
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
