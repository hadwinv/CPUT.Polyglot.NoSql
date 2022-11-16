using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public class Neo4jPart : Transcriber
    {
        private List<NSchema> _schemas;
        public Neo4jPart(List<NSchema> schemas)
        {
            _schemas = schemas;
        }

        public override Constructs Execute(CreatePart request)
        {
            StrategyPart strategy = new Neo4jStrategy();

            var native = strategy.Query(request, _schemas);

            return new Constructs
            {
                Target = Database.NEOJ4,
                Query = native,
                Expression = request.BaseExpr,
            };
        }
    }
}