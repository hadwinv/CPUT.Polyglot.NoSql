using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators
{
    public class DirectionPart : IExpression
    {
        internal string Type { get; set; }

        public DirectionPart(DirectionType direction)
        {
            Dictionary<DirectionType, string> keywords;

            keywords = new Dictionary<DirectionType, string>
            {
                [DirectionType.None] = string.Empty,
                [DirectionType.Asc] = "ASC",
                [DirectionType.Desc] = "DESC"
            };

            Type = keywords[direction];
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
