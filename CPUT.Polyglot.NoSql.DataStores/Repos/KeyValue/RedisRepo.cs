using Amazon.Auth.AccessControlPolicy;
using App.Metrics;
using App.Metrics.Timer;
using Cassandra;
using CPUT.Polyglot.NoSql.Common.Reporting;
using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Mapper.ViewMap;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Native._data.prep.Redis;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using CPUT.Polyglot.NoSql.Models.Views.Bindings;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Models.Views.Unified;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.KeyValue
{
    public class RedisRepo : IRedisRepo
    {
        private IRedisBridge _connector;

        private readonly ITimer _timer;

        public RedisRepo(IRedisBridge connector)
        {
            _connector = connector;
        }

        public Models.Result Execute(QueryDirective query)
        {
            Models.Result result = null;

            var redis = _connector.Connect();

            if (query.Executable != null)
            {
                RedisResult results = null;

                var data = new List<ResultsModel>();

                var statements = query.Executable.Split(";");

                var user = new rUser();
                var type = typeof(rUser);

                foreach (var statement in statements)
                {
                    //execute command
                    var parts = statement.Split("|");

                    if (parts[0] == "SET")
                    {
                        var keyvalues = parts[1].Split("%");

                        foreach (var expression in keyvalues[1].Split(","))
                        {
                            var fields = expression.Split("=");

                            var propertyInfo = type.GetProperty(fields[0]);

                            if (propertyInfo != null)
                            {
                                propertyInfo.SetValue(user, fields[1], null);
                            }
                        }
                        results = redis.Execute(parts[0].Trim(), new object[] { keyvalues[0].Trim(), JsonConvert.SerializeObject(user) });
                    }
                    else
                    {
                        results = redis.Execute(parts[0].Trim(), parts[1].Trim());
                    }


                    if (!results.IsNull)
                    {
                        var response = new List<object>();

                        bool success = true;

                        var settings = new JsonSerializerSettings
                        {
                            Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                            MissingMemberHandling = MissingMemberHandling.Error
                        };

                        if (results.Type == ResultType.BulkString)
                        {
                            var record = ((RedisValue)results);

                            var @object = JsonConvert.DeserializeObject<object>(record, settings);

                            if (success)
                                response.Add(@object);

                        }
                        else if (results.Type == ResultType.MultiBulk)
                        {
                            var keys = ((RedisKey[])results);

                            foreach (var key in keys)
                            {
                                var record = redis.StringGet(key);

                                var @object = JsonConvert.DeserializeObject<object>(record, settings);

                                if (success)
                                    response.Add(@object);
                            }
                        }

                        if (query.Command == Common.Helpers.Utils.Command.MODIFY)
                        {
                            if (parts[0] == "GET")
                                user = JsonConvert.DeserializeObject<rUser>(results.ToString());
                        }
                        else
                        {
                            //check if codex instructions were configured
                            if (query.Codex != null)
                                data.AddRange(ModelBuilder.Create(query.Codex, response));
                        }
                    }
                }

                result = new Models.Result
                {
                    Source = Common.Helpers.Utils.Database.REDIS,
                    Data = data,
                    Status = "OK",
                    Message = query.Command != Common.Helpers.Utils.Command.FETCH 
                                    ?string.Format("{0} executed sucessfully", Enum.GetName(typeof(Common.Helpers.Utils.Command), query.Command).ToUpper()) :
                                                string.Format("Query returned {0} record(s)", data.Count),
                    Success = true
                };
            }

            return result;
        }

    }
}
