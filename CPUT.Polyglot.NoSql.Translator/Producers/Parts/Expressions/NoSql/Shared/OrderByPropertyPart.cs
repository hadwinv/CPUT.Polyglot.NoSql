﻿using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared
{
    public class OrderByPropertyPart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal DirectionPart Direction { get; set; }

        public OrderByPropertyPart(Link mappedProperty, OrderByPropertyExpr expr)
        {
            Name = mappedProperty.Property;
            AliasIdentifier = expr.AliasIdentifier;

            if (string.IsNullOrEmpty(AliasIdentifier))
                AliasIdentifier = mappedProperty.Reference.Substring(0, 3).ToLower();

            Direction = new DirectionPart(expr.Direction);

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
