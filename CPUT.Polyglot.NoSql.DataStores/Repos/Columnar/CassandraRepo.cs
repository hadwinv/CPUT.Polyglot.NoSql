using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Mapper.ViewMap;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Columnar
{
    public class CassandraRepo : ICassandraRepo
    {
        private ICassandraBridge _session;

        public CassandraRepo(ICassandraBridge session)
        {
            _session = session;
        }

        public Models.Result Execute(QueryDirective query)
        {
            Models.Result result = null;
            dynamic data = null;

            try
            {
                if (query.Executable != null)
                {
                    var response = _session.Connect().Execute(query.Executable);

                    //check if codex instructions were configured
                    if(query.Codex != null)
                        data = ModelBuilder.Create(query.Codex, response);
                    
                    result = new Models.Result
                    {
                        Source = Common.Helpers.Utils.Database.CASSANDRA,
                        Data = data,
                        Status = "OK",
                        Message = data != null ? string.Format("Query returned {0} record(s)", data.Count) : string.Format("{0} executed sucessfully", Enum.GetName(typeof(Command), query.Command).ToUpper()),
                        Success = true
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");

                result = new Models.Result
                {
                    Source = Common.Helpers.Utils.Database.CASSANDRA,
                    Data = data,
                    Status = "Error",
                    Message = ex.Message,
                    Success = false
                };
            }
            finally
            {
                if (_session != null)
                    _session.Disconnect();
            }

            return result;
        }
    }
}
