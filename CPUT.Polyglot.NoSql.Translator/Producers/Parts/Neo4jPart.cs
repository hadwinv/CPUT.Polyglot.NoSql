using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;
using CPUT.Polyglot.NoSql.Models.Views;
using App.Metrics;

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
                Result = query,
                Message = string.IsNullOrEmpty(query.Query) ? "Unable to generate Neo4j query." : "Neo4j query generated.",
                Success = !string.IsNullOrEmpty(query.Query)
            };
        }
    }
}