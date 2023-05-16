using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class FindPart : IExpression
    {
        internal ConditionPart? Condition { get; set; }

        internal FieldPart? Field { get; set; }
        
        internal OrderByPart? OrderBy { get; set; }

        internal RestrictPart? Restrict { get; set; }

        public FindPart()
        {

        }
        //public FindPart(ConditionPart? condition, FieldPart? field, OrderByPart? orderBy, RestrictPart? restrict)
        //{
        //    Condition = condition;
        //    Field = field;
        //    OrderBy = orderBy;
        //    Restrict = restrict;
        //}

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
