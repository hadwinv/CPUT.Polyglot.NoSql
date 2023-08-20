using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Models.Translator;
using Superpower.Model;

namespace CPUT.Polyglot.NoSql.Logic.Core.Handler
{
    public abstract class CommandHandler : ICommandHandler
    {
        private ICommandHandler _handler;
        private ITranslate _translate;
        private IValidator _validator;
        

        public CommandHandler(IValidator validator, ITranslate translate) 
        {
            _validator = validator;
            _translate = translate;
        }

        public virtual Output Execute(TokenList<Lexicons> request)
        {
            if (this._handler != null)
            {
                return this._handler.Execute(request);
            }
            else
            {
                return null;
            }
        }
    }
}
