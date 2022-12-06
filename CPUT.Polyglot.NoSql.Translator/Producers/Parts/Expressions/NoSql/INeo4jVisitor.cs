using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql
{
    public interface INeo4jVisitor
    {
        void Visit(QueryPart query);

        void Visit(RelationshipPart relationship);

        void Visit(NodePart dataModel);

        void Visit(PropertyPart property);

        void Visit(ConditionPart condition);

        void Visit(LogicalPart where);

        void Visit(ReturnPart @return);

        void Visit(RestrictPart restrict);

        void Visit(SeparatorPart separator);

        void Visit(OperatorPart operatorPart);
        
        void Visit(ComparePart comparePart);
        
        void Visit(MatchPart matchPart);
        
        void Visit(SetPart setPart);
        
        void Visit(SetValuePart setValuePart);
        
        void Visit(InsertPart insertPart);
        
        void Visit(ValuesPart valuesPart);
        
        void Visit(InsertNodePart insertNodePart);
        
        void Visit(OrderByPart orderByPart);
        
        void Visit(DirectionPart directionPart);
        
        void Visit(NFunctionPart functionPart);
        
        void Visit(UnwindPart unwindPart);
        
        void Visit(UnwindPropertyPart unwindPropertyPart);
        
        void Visit(UnwindJsonPart unwindJsonPart);
    }
}
