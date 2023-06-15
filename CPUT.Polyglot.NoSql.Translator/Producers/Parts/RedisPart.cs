using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public class RedisPart : Transcriber
    {
        public RedisPart() {}

        public override Constructs Execute(CreatePart request)
        {
            //get query parts
            var query = new RedisStrategy().Query(request);

            return new Constructs
            {
                Target = Database.REDIS,
                Query = query,
                Expression = request.BaseExpr
            };
        }
    }
}