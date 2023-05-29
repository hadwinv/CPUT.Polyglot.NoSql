﻿using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public class RedisPart : Transcriber
    {
        public List<USchema> _uSchema { get; set; }
        public List<NSchema> _nSchema { get; set; }

        public RedisPart(List<USchema> uSchema, List<NSchema> nSchema)
        {
            _uSchema = uSchema;
            _nSchema = nSchema;
        }

        public override Constructs Execute(CreatePart request)
        {
            StrategyPart strategy = new RedisStrategy();

            //set schemas
            Assistor.USchema = _uSchema;
            Assistor.Add((int)Database.REDIS, _nSchema);

            //get query parts
            var query = strategy.Query(request);

            return new Constructs
            {
                Target = Database.REDIS,
                Query = query,
                Expression = request.BaseExpr
            };
        }
    }
}