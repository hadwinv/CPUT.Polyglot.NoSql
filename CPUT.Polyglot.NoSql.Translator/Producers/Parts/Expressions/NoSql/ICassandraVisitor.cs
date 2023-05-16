using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Cassandra;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql
{
    public interface ICassandraVisitor
    {
        void Visit(QueryPart queryExpr);

        void Visit(SelectPart select);

        void Visit(FromPart from);

        void Visit(PropertyPart property);

        void Visit(ConditionPart condition);

        void Visit(LogicalPart where);

        void Visit(RestrictPart restrict);

        void Visit(SeparatorPart separator);
        void Visit(OperatorPart operatorPart);
        void Visit(ComparePart comparePart);
        void Visit(TablePart tablePart);
        void Visit(UpdatePart updatePart);
        void Visit(SetPart setPart);
        void Visit(SetValuePart setValuePart);
        void Visit(InsertPart insertPart);
        void Visit(ValuesPart valuesPart);
        void Visit(InsertValuePart insertValuePart);
        void Visit(DirectionPart directionPart);
        void Visit(OrderByPart orderByPart);
        void Visit(NativeFunctionPart nFunctionPart);
    }
}
