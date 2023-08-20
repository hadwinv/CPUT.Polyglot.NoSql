using App.Metrics;
using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Common.Reporting;
using CPUT.Polyglot.NoSql.DataStores.Repos._data;
using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Logic.Core.DML;
using CPUT.Polyglot.NoSql.Logic.Core.Events;
using CPUT.Polyglot.NoSql.Models;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Logic
{
    public class ServiceLogic : IServiceLogic
    {
        private ICommandEvent _commandEvent;
        private IExecutor _executor;
        private IMockData _mockData;
        private IMetrics _metrics;

        public ServiceLogic(ICommandEvent commandEvent, 
            ITranslate translate,
            IValidator validator,
            IExecutor executor,
            IMockData mockData,
            IMetrics metrics)
        {
            _commandEvent = commandEvent;
            _executor = executor;
            _mockData = mockData;
            _metrics = metrics;

            //handlers to construct queries
            _commandEvent.Add((int)Utils.Command.FETCH, new FetchHandler(validator, translate));
            _commandEvent.Add((int)Utils.Command.MODIFY, new ModifyHandler(validator, translate));
            _commandEvent.Add((int)Utils.Command.ADD, new AddHandler(validator, translate));
        }


        public void GenerateData()
        {
            try
            {
                _mockData.GenerateData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
        }

        public void DataLoad()
        {
            try
            {
                _mockData.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
        }

        public List<Models.Result> Query(string input)
        {
            List<Models.Result> results = null;

            try
            {
                using (_metrics.Measure.Apdex.Track(AppMetricsRegistry.ApdexScores.PolyglotNoSqlApdex))
                {
                    //get quey model
                    var query = GetQueryModel(input);

                    //check if system was able to determine the command type
                    if (query.Command != Utils.Command.NONE)
                    {
                        //run command to generate native query
                        var output = _commandEvent.Run(query);

                        if (output != null)
                        {
                            //send native query for execution
                            results = _executor.Forward(query.Command, output).Result;
                        }
                    }
                    else
                    {
                        results = new List<Models.Result>
                        {
                            new Result
                            {
                                Success = false,
                                Message = "Invalid Command",
                                Status = "Failed"
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                results = new List<Models.Result>
                        {
                            new Result
                            {
                                Success = false,
                                Message = ex.Message,
                                Status = "Error"
                            }
                        };

                Console.WriteLine($"Exception - {ex.Message}");
            }

            return results;
        }

        private Query GetQueryModel(string input)
        {
            Query query = new Query();
            try
            {
                query.Command = GetCommand(input);
                query.Tokens = new Lexer().Tokenize(input);
            }
            catch (Exception ex)
            {
                query.Message = ex.Message;
            }
            return query;
        }

        private Utils.Command GetCommand(string query)
        {
            if (query.ToUpper().Contains("FETCH"))
            {
                return Utils.Command.FETCH;
            }
            else if (query.ToUpper().Contains("ADD"))
            {
                return Utils.Command.ADD;
            }
            else if (query.ToUpper().Contains("MODIFY"))
            {
                return Utils.Command.MODIFY;
            }

            return Utils.Command.NONE;
        }


    }
}
