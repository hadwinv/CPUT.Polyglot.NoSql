using App.Metrics;
using Cassandra;
using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Mapper.ViewMap;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using Neo4j.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static System.Collections.Specialized.BitVector32;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Graph
{
    public class Neo4jRepo : INeo4jRepo
    {
        private readonly INeo4jBridge _connector;

        public Neo4jRepo(INeo4jBridge connector) 
        {
            _connector = connector;
        }

        public Models.Result Execute(QueryDirective query)
        {
            Models.Result result = null;

            var connection = _connector.Connect();

            var session = connection.AsyncSession(configBuilder => configBuilder.WithDatabase("enrollmentdb"));

            if (query.Executable != null)
            {
                dynamic data = null;

                if (query.Command == Utils.Command.FETCH)
                {
                    var results = session.ReadTransactionAsync(async tx =>
                    {
                        var cursor = await tx.RunAsync(query.Executable);

                        var records = await cursor.ToListAsync();

                        //codex instructions for results
                        return ModelBuilder.Create(query.Codex, records);
                    });

                    data = results.Result;
                }
                else
                {
                    var results = session.WriteTransactionAsync(async tx =>
                    {
                        return await tx.RunAsync(query.Executable);
                    }).Result;

                }

                result = new Models.Result
                {
                    Source = Common.Helpers.Utils.Database.NEO4J,
                    Data = data,
                    Status = "OK",
                    Message = data != null ? string.Format("Query returned {0} record(s)", data.Count) : string.Format("{0} executed sucessfully", Enum.GetName(typeof(Command), query.Command).ToUpper()),
                    Success = true
                };
            }

            return result;
        }
    }
}
