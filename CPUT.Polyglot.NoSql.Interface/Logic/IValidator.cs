using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Interface.Logic
{
    public interface IValidator
    {
        Validators GlobalSchema(BaseExpr baseExpr);
    }
}
