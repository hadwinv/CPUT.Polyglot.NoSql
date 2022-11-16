using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;

namespace CPUT.Polyglot.NoSql.Translator.Producers
{
    public abstract class Transcriber : ITranscriber
    {
        public virtual DeclareExpr DeclareExpr { set; get; }
        public virtual DataModelExpr DataModelExpr { set; get; }
        public LinkExpr? LinkExpr { get; set; }
        public FilterExpr? FilterExpr { get; set; }
        public GroupByExpr? GroupByExpr { get; set; }
        public RestrictExpr? RestrictExpr { get; set; }

        private ITranscriber _transcriber;

        public Transcriber(){}

        public virtual Constructs Execute(CreatePart request)
        {
            if (this._transcriber != null)
            {
                return this._transcriber.Execute(request);
            }
            else
            {
                return null;
            }
        }
    }
}
