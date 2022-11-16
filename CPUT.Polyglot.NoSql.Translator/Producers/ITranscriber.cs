using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;

namespace CPUT.Polyglot.NoSql.Translator.Producers
{
    public interface ITranscriber
    {
        Constructs Execute(CreatePart request);
    }
}
