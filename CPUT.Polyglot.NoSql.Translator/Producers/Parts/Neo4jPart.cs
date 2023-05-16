using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;
using CPUT.Polyglot.NoSql.Models.Views;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public class Neo4jPart : Transcriber
    {
        public List<USchema> _uSchema { get; set; }
        public List<NSchema> _nSchema { get; set; }

        public Neo4jPart(List<USchema> uSchema, List<NSchema> nSchema)
        {
            _uSchema = uSchema;
            _nSchema = nSchema;
        }

        public override Constructs Execute(CreatePart request)
        {
            StrategyPart strategy = new Neo4jStrategy();

            //set schemas
            Assistor.USchema = _uSchema;
            Assistor.NSchema = _nSchema;

            //get query parts
            var native = strategy.Query(request);

            return new Constructs
            {
                Target = Database.NEOJ4,
                Query = native,
                Expression = request.BaseExpr,
            };
        }
    }
}