using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Models.Translator;
using Superpower.Model;
using System;

namespace CPUT.Polyglot.NoSql.Logic.Core.Handler
{
    public class DescribeHandler : CommandHandler
    {
        private ITranslate _translate;

        public DescribeHandler(IValidator validator, ITranslate translate) : base(validator, translate)
        {
            _translate = translate;
        }


        public override Output Execute(TokenList<Lexicons> request)
        {
            Output result = null;

            try
            {
               
            }
            catch (Exception ex)
            {
            }

            return result;
        }
    }
}
