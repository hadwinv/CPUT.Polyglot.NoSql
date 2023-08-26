using App.Metrics;
using App.Metrics.Timer;
using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Common.Reporting;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Logic.Core.Handler;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using Superpower;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUT.Polyglot.NoSql.Logic.Core.DML
{
    public class FetchHandler : CommandHandler
    {
        private IValidator _validator;
        private ITranslate _translate;

        private static IMetrics _metrics;

        public FetchHandler(IValidator validator, ITranslate translate, IMetrics metrics) : base(validator, translate)
        {
            _validator = validator;
            _translate = translate;
            
            _metrics = metrics;
        }

        public override Output Execute(TokenList<Lexicons> request)
        {
            var constructs = new List<Constructs>();

            try
            {
                Validators validatorResult = null;
                BaseExpr syntaxExpr = null;

                var _timer = _metrics.Provider.Timer.Instance(MetricsRegistry.Calls.Parser);

                using (var context = _timer.NewContext("Fetch Parser"))
                {
                    try
                    {
                        //generate abstract syntax tree
                        syntaxExpr = Expressions.FETCH.Parse(request);

                        //validator syntax tree against global
                        validatorResult = _validator.GlobalSchema(syntaxExpr);
                    }
                    catch
                    {
                        _metrics.Measure.Counter.Increment(MetricsRegistry.Errors.Parser);
                    }
                }

                //verify if query passed globa schema
                if (validatorResult != null && validatorResult.Success)
                {
                    //convert to native queries
                    var transformed = _translate.Convert(
                            new ConstructPayload
                            {
                                BaseExpr = syntaxExpr,
                                Command = Utils.Command.FETCH
                            });

                    constructs = transformed.Result;
                }
                else
                {
                    if(validatorResult != null )
                    {
                        constructs.Add(new Constructs
                        {
                            Success = validatorResult.Success,
                            Message = validatorResult.Message
                        });
                    }
                    else
                    {
                        constructs.Add(new Constructs
                        {
                            Success = false,
                            Message = "Syntax error occurred."
                        });
                    }
                }
            }
            catch
            {
                _metrics.Measure.Counter.Increment(MetricsRegistry.Errors.General);
            }
            return new Output
            {
                Constructs = constructs
            };
        }
    }
}
