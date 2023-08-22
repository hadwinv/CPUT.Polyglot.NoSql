using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Logic.Core.Handler;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser;
using Superpower.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using Superpower;
using App.Metrics;
using CPUT.Polyglot.NoSql.Common.Reporting;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Logic.Core.DML
{
    public class AddHandler : CommandHandler
    {
        private IValidator _validator;
        private ITranslate _translate;

        private static IMetrics _metrics;

        public AddHandler(IValidator validator, ITranslate translate, IMetrics metrics) : base(validator, translate)
        {
            _validator = validator;
            _translate = translate;

            _metrics = metrics;
        }

        public override Output Execute(TokenList<Lexicons> request)
        {
            List<Constructs> constructs = null;

            try
            {
                Validators validatorResult = null;
                BaseExpr syntaxExpr = null;

                var _timer = _metrics.Provider.Timer.Instance(MetricsRegistry.Calls.Parser);

                using (var context = _timer.NewContext("ADD Parser"))
                {
                    try
                    {
                        //generate abstract syntax tree
                        syntaxExpr = Expressions.ADD.Parse(request);

                        //validator syntax tree against global
                        validatorResult = _validator.GlobalSchema(syntaxExpr);
                    }
                    catch
                    {
                        _metrics.Measure.Counter.Increment(MetricsRegistry.Errors.Parser);
                    }
                }

                //verify if query passed globa schema
                if (validatorResult.Success)
                {
                    //convert to native queries
                    var transformed = _translate.Convert(
                             new ConstructPayload
                             {
                                 BaseExpr = syntaxExpr,
                                 Command = Utils.Command.ADD
                             });

                    constructs = transformed.Result;
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
