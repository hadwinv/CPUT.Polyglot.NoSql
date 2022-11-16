using CPUT.Polyglot.NoSql.Models.Translator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Interface.Translator
{
    public interface ITranslate
    {
        Task<List<Constructs>> Convert (ConstructPayload payload);
    }
}
