﻿using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared
{
    public class OrderByPart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal DirectionPart Direction { get; set; }

        public OrderByPart(Link mappedProperty, OrderByExpr orderByExpr)
        {
            Name = mappedProperty.Property;
            AliasIdentifier = orderByExpr.AliasIdentifier;

            if (string.IsNullOrEmpty(AliasIdentifier))
                AliasIdentifier = mappedProperty.Reference.Substring(0, 3).ToLower();

            Direction = new DirectionPart(orderByExpr.Direction);
            
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
            throw new NotImplementedException();
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
