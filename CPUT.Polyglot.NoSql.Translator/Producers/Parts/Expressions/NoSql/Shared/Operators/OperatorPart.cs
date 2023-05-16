using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators
{
    public class OperatorPart : IExpression
    {
        internal string Type { get; set; }

        public OperatorPart(OperatorType @operator, Database targetDb)
        {
            Dictionary<OperatorType, string> keywords;

            if(targetDb == Database.MONGODB)
            {
                keywords = new Dictionary<OperatorType, string>
                {
                    [OperatorType.Gtr] = "$gt",
                    [OperatorType.Gte] = "$gte",
                    [OperatorType.Lss] = "$lt",
                    [OperatorType.Lte] = "$lte",
                    [OperatorType.Eql] = ":"
                };
            }
            else
            {
                keywords = new Dictionary<OperatorType, string>
                {
                    [OperatorType.Gtr] = ">",
                    [OperatorType.Gte] = ">=",
                    [OperatorType.Lss] = "<",
                    [OperatorType.Lte] = "<=",
                    [OperatorType.Eql] = "="
                };
            }

            Type = keywords[@operator];
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
