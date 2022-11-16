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
    public class ComparePart : IExpression
    {
        internal string Type { get; set; }

        public ComparePart(CompareType compare, Database targetDb)
        {
            Dictionary<CompareType, string> keywords;

            if (targetDb == Database.MONGODB)
            {
                keywords = new Dictionary<CompareType, string>
                {
                    [CompareType.None] = string.Empty,
                    [CompareType.And] = "$and",
                    [CompareType.Or] = "$or"
                };
            }
            else
            {
                keywords = new Dictionary<CompareType, string>
                {
                    [CompareType.None] = string.Empty,
                    [CompareType.And] = "AND",
                    [CompareType.Or] = "OR"
                };
            }

            Type = keywords[compare];
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
