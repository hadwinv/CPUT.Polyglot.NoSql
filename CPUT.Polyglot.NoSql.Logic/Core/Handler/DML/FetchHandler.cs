using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Common.Parsers;
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

        public FetchHandler(IValidator validator, ITranslate translate) : base(validator, translate)
        {
            _validator = validator;
            _translate = translate;
        }

        public override Output Execute(TokenList<Lexicons> request)
        {
            var constructs = new List<Constructs>();

            try
            {
                //generate abstract syntax tree
                var syntaxExpr = Expressions.FETCH.Parse(request);

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
                else
                {
                    constructs.Add(new Constructs
                    {
                        Success = validatorResult.Success,
                        Message = validatorResult.Message
                    });
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
