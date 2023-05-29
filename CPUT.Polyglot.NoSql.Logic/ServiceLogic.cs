using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Logic.Core.DML;
using CPUT.Polyglot.NoSql.Logic.Core.Events;
using CPUT.Polyglot.NoSql.Models;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using System;

namespace CPUT.Polyglot.NoSql.Logic
{
    public class ServiceLogic : IServiceLogic
    {
        private ICommandEvent _commandEvent;
        private IProxy _proxy;

        public ServiceLogic(ICommandEvent commandEvent, 
            ITranslate translate,
            IValidator validator,
            IProxy proxy)
        {
            _commandEvent = commandEvent;
            _proxy = proxy;

            //handlers to construct queries
            _commandEvent.Add((int)Utils.Command.FETCH, new FetchHandler(validator, translate));
            _commandEvent.Add((int)Utils.Command.MODIFY, new ModifyHandler(validator, translate));
            _commandEvent.Add((int)Utils.Command.ADD, new AddHandler(validator, translate));
        }

        public void DataLoad()
        {
            try
            {
                _proxy.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
        }

        public Models.Result Query(string input)
        {
            Models.Result result = null;
            Query query = null;

            try
            {
                //get quey model
                query = GetQueryModel(input);

                //check if system was able to determine the command type
                if(query.Command != Utils.Command.NONE)
                {
                    var output = _commandEvent.Run(query);

                    foreach(var target in output.Constructs)
                        result = _proxy.Forward(target);
                }
                else
                {
                    //error or invalid
                    result = new Models.Result
                    {
                        Success = false,
                        Message = "Invalid Command",
                    };
                }
            }
            catch (Exception ex)
            {
                result = new Models.Result
                {
                    Success = false,
                    Message = ex.Message,
                };

                Console.WriteLine($"Exception - {ex.Message}");
            }

            return result;
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
