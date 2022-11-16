using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Logic.Core.Handler;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser;
using Superpower;
using Superpower.Model;
using System;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Logic.Core.DML
{
    public class FetchHandler : CommandHandler
    {
        private IValidator _validator;
        private ITranslate _translate;
        private ISchema _schema;

        public FetchHandler(IValidator validator, ITranslate translate) : base(validator, translate)
        {
            _validator = validator;
            _translate = translate;
        }

        public override Output Execute(TokenList<Lexicons> request)
        {
            List<Constructs> constructs = null;

            try
            {
                //generate abstract syntax tree
                var syntaxExpr = Expressions.Select.Parse(request);

                //validator syntax tree against global
                var validatorResult = _validator.GlobalSchema(syntaxExpr);

                //verify if query passed globa schema
                if (validatorResult.Success)
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
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return new Output
            {
                Constructs = constructs
            }; ;
        }
    }
}
