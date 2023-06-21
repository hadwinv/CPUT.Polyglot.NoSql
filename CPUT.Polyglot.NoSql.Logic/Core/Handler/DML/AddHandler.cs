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

namespace CPUT.Polyglot.NoSql.Logic.Core.DML
{
    public class AddHandler : CommandHandler
    {
        private IValidator _validator;
        private ITranslate _translate;

        public AddHandler(IValidator validator, ITranslate translate) : base(validator, translate)
        {
            _translate = translate;
            _translate = translate;
        }

        public override Output Execute(TokenList<Lexicons> request)
        {
            List<Constructs> constructs = null;

            try
            {
                //generate abstract syntax tree
                var syntaxExpr = Expressions.ADD.Parse(request);

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
                                 Command = Utils.Command.ADD
                             });

                    constructs = transformed.Result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return new Output
            {
                Constructs = constructs
            };
        }
    }
}
