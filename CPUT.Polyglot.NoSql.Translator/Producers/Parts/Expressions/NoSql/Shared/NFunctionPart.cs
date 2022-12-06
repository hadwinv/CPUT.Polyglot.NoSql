using Cassandra.Mapping;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared
{
    public class NFunctionPart : IExpression
    {
        internal IExpression PropertyPart { get; set; }

        internal string Type { get; set; }

        internal Dictionary<AggregateType, string> _keywords = new Dictionary<AggregateType, string>
        {
            [AggregateType.None] = "",
            [AggregateType.Sum] = "SUM",
            [AggregateType.Count] = "COUNT",
            [AggregateType.Avg] = "AVG",
            [AggregateType.Max] = "MAX",
            [AggregateType.Min] = "MIN",
            [AggregateType.NSum] = "SUM",
            [AggregateType.NCount] = "COUNT",
            [AggregateType.NAvg] = "AVG",
            [AggregateType.NMax] = "MAX",
            [AggregateType.NMin] = "MIN"
        };

        public NFunctionPart(IExpression propertyPart, AggregateType type)
        {
            PropertyPart = propertyPart;
            Type = _keywords[type];
        }

        public void Accept(INeo4jVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(ICassandraVisitor visitor)
        {
            //visitor.Visit(this);
        }

        public void Accept(IRedisVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            //visitor.Visit(this);
        }
    }
}
