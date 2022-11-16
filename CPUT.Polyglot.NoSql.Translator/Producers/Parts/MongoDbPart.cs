using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public class MongoDbPart : Transcriber
    {
        private List<NSchema> _schemas;

        public MongoDbPart(List<NSchema> schemas)
        {
            _schemas = schemas;
        }

        public override Constructs Execute(CreatePart request)
        {
            StrategyPart strategy = new MongoDbStrategy();

            //get query parts
            var query = strategy.Query(request, _schemas);

            return new Constructs
            {
                Target = Database.MONGODB,
                Query = query,
                Expression = request.BaseExpr
            };
        }
    }
}
