using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Translator.Producers;

namespace CPUT.Polyglot.NoSql.Translator.Events
{
    public class Interpreter : IInterpreter
    {
        public Dictionary<int, Transcriber> Query { get; set; }

        public Interpreter()
        {
            Query = new Dictionary<int, Transcriber>();
        }

        public void Add(int index, Transcriber handler)
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
                        Mapper = request.Mapper,
                        Command = request.Command
                    });
            }

            return constructs;
        }
    }
}
