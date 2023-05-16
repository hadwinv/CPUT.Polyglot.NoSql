using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql
{
    public interface IMongoDbVisitor
    {
        void Visit(QueryPart query);
        void Visit(PropertyPart property);
        void Visit(LogicalPart where);
        void Visit(RestrictPart restrict);
        void Visit(SeparatorPart separator);
        void Visit(ConditionPart condition);
        void Visit(CollectionPart collection);
        void Visit(FindPart find);
        void Visit(FieldPart field);
        void Visit(OperatorPart operatorPart);
        void Visit(ComparePart comparePart);
        void Visit(SetPart setPart);
        void Visit(UpdatePart updatePart);
        void Visit(SetValuePart setValuesPart);
        void Visit(InsertPart insertPart);
        void Visit(AddPart addPart);
        void Visit(DirectionPart directionPart);
        void Visit(OrderByPart orderByPart);
        void Visit(AggregatePart aggregatePart);
        void Visit(MatchPart matchPart);
        void Visit(UnwindPart unwindPart);
        void Visit(GroupByPart groupPart);
        void Visit(ProjectPart projectPart);
        void Visit(UnwindJsonPart unwindJsonPart);
        void Visit(ProjectFieldPart projectPropertyPart);
        void Visit(GroupByFieldPart groupByFieldPart);
        void Visit(NativeFunctionPart nFunctionPart);
        void Visit(FunctionFieldPart functionPropertyPart);
    }
}
