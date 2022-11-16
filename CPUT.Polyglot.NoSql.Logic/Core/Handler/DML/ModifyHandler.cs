using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Logic.Core.Handler;
using CPUT.Polyglot.NoSql.Models.Translator;
using Superpower.Model;

namespace CPUT.Polyglot.NoSql.Logic.Core.DML
{
    public class ModifyHandler : CommandHandler
    {
        private ITranslate _translate;

        public ModifyHandler(IValidator validator, ITranslate translate) : base(validator, translate)
        {
            _translate = translate;
        }

        public override Output Execute(TokenList<Lexicons> request)
        {
            Output result = null;

            //ast = Expressions.Update.Parse(tokens);

            return result;
        }
    }
}
