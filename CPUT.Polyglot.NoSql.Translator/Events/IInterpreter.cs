using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Translator.Producers;

namespace CPUT.Polyglot.NoSql.Translator.Events
{
    public interface IInterpreter
    {
        void Add(Common.Helpers.Utils.Database index, Transcriber handler);
        Constructs Run(Enquiry request);
    }
}
