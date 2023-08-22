using App.Metrics;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public class CassandraPart : Transcriber
    {
        public CassandraPart() {}

        public override Constructs Execute(CreatePart request)
        {
            //get query parts
            var query = new CassandraStrategy().Query(request);

            return new Constructs
            {
                Target = Database.CASSANDRA,
                Result = query,
                Message = string.IsNullOrEmpty(query.Query) ? "Unable to generate Cassandra query." : "Cassandra query generated.",
                Success = !string.IsNullOrEmpty(query.Query)
            };
        }
    }
}
