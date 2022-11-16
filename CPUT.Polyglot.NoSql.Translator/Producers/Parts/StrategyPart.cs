using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public abstract class StrategyPart 
    {
        public abstract string Fetch(BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas);

        public abstract string Modify(BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas);

        public abstract string Add(BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas);

        public abstract string Alter();

        public abstract string Create();

        public abstract string Describe();

        public string Query(CreatePart request, List<NSchema> schemas)
        {
            //make a copy of incoming expression request
            if (request.Command == Command.FETCH)
            {
                return Fetch(request.BaseExpr, request.Mapper, schemas);
            }
            else if (request.Command == Command.ADD)
            {
                return Add(request.BaseExpr, request.Mapper, schemas);
            }
            else if (request.Command == Command.MODIFY)
            {
                return Modify(request.BaseExpr, request.Mapper, schemas);
            }
            else if (request.Command == Command.ALTER)
            {
                return Alter();
            }
            else if (request.Command == Command.CREATE)
            {
                return Create();
            }
            else if (request.Command == Command.DESCRIBE)
            {
                return Describe();
            }

            return string.Empty;
        }

        public Link GetMappedProperty(List<MappedSource> mapperLinks, string property, string target)// string dataModel,
        {
            return mapperLinks
                          .SelectMany(x => x.Resources
                                            .Where(y => y.Source == property)
                                            .SelectMany(x => x.Link))
                           .FirstOrDefault(t => t.Target == target);//&& t.Reference == dataModel
        }

    }
}
