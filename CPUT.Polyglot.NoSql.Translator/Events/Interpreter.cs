using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Translator.Producers;

namespace CPUT.Polyglot.NoSql.Translator.Events
{
    public class Interpreter : IInterpreter
    {
        public Dictionary<Common.Helpers.Utils.Database, Transcriber> Query { get; set; }

        public Interpreter()
        {
            Query = new Dictionary<Common.Helpers.Utils.Database, Transcriber>();
        }

        public void Add(Common.Helpers.Utils.Database index, Transcriber handler)
        {
            Query.Add(index, handler);
        }

        public Constructs Run(Enquiry request)
        {
            Constructs constructs = null;

            if (Query.ContainsKey(request.Database))
            {
                constructs = Query[request.Database].Execute(
                    new Models.Translator.Parts.CreatePart{
                        BaseExpr = request.BaseExpr,
                        Command = request.Command
                    });
            }

            return constructs;
        }
    }
}
