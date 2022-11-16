using CPUT.Polyglot.NoSql.Models.Translator;

namespace CPUT.Polyglot.NoSql.Interface.Delegator
{
    public interface IProxy
    {
        Models.Result Forward(Constructs construct);

        void Load();
    }
}
