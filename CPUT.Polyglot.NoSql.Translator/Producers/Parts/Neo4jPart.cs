using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;
using CPUT.Polyglot.NoSql.Models.Views;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public class Neo4jPart : Transcriber
    {
        public Neo4jPart() {}

        public override Constructs Execute(CreatePart request)
        {
            //get query parts
            var query = new Neo4jStrategy().Query(request);

            return new Constructs
            {
                Target = Database.NEO4J,
                Query = query,
                Expression = request.BaseExpr,
            };
        }
    }
}