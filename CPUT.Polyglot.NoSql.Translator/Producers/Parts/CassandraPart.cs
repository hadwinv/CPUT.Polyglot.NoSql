using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public class CassandraPart : Transcriber
    {
        private List<NSchema> _schemas;

        public CassandraPart(List<NSchema> schemas)
        {
            _schemas = schemas;
        }

        public override Constructs Execute(CreatePart request)
        {
            StrategyPart strategy = new CassandraStrategy();

            var query = strategy.Query(request, _schemas);

            return new Constructs
            {
                Target = Database.CASSANDRA,
                Query = query,
                Expression = request.BaseExpr
            };
        }
    }
}
