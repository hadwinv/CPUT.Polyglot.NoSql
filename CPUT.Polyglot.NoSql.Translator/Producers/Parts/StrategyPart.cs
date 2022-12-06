using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using System.ComponentModel;
using System.Linq.Expressions;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts
{
    public abstract class StrategyPart 
    {
        public DeclareExpr DeclareExpr { get; set; }
        public PropertiesExpr PropertiesExpr { get; set; }
        public DataModelExpr DataModelExpr { get; set; }
        public LinkExpr LinkExpr { get; set; }
        public FilterExpr FilterExpr { get; set; }
        public RestrictExpr RestrictExpr { get; set; }
        public OrderByExpr OrderByExpr { get; set; }

        public abstract string Fetch(BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas);

        public abstract string Modify(BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas);

        public abstract string Add(BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas);

        public abstract string Alter();

        public abstract string Create();

        public abstract string Describe();

        public string Query(CreatePart request, List<NSchema> schemas)
        {
            //set expression parts
            this.DeclareExpr = (DeclareExpr)request.BaseExpr.ParseTree.Single(x => x.GetType().Equals(typeof(DeclareExpr)));
            this.PropertiesExpr = (PropertiesExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(PropertiesExpr)));
            this.DataModelExpr = (DataModelExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(DataModelExpr)));
            this.LinkExpr = (LinkExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(LinkExpr)));
            this.FilterExpr = (FilterExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(FilterExpr)));
            this.RestrictExpr = (RestrictExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(RestrictExpr)));
            this.OrderByExpr = (OrderByExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(OrderByExpr)));

            //make a copy of incoming expression request
            if (request.Command == Command.FETCH)
                return Fetch(request.BaseExpr, request.Mapper, schemas);
            else if (request.Command == Command.ADD)
                return Add(request.BaseExpr, request.Mapper, schemas);
            else if (request.Command == Command.MODIFY)
                return Modify(request.BaseExpr, request.Mapper, schemas);
            else if (request.Command == Command.ALTER)
                return Alter();
            else if (request.Command == Command.CREATE)
                return Create();
            else if (request.Command == Command.DESCRIBE)
                return Describe();

            return string.Empty;
        }

        public Link GetMappedProperty(List<MappedSource> mapperLinks, BaseExpr baseExpr, string target)
        {
            DataExpr dataExpr = null;
            
            string value = string.Empty;
            string aliasIndentifier = string.Empty;

            if (baseExpr is PropertyExpr)
            {
                value = ((PropertyExpr)baseExpr).Value;
                aliasIndentifier = ((PropertyExpr)baseExpr).AliasIdentifier;
            }
            else if (baseExpr is TermExpr)
            {
                value = ((TermExpr)baseExpr).Value;
                aliasIndentifier = ((TermExpr)baseExpr).AliasIdentifier;
            }
            else if (baseExpr is OrderByExpr)
            {
                value = ((OrderByExpr)baseExpr).Value;
                aliasIndentifier = ((OrderByExpr)baseExpr).AliasIdentifier;
            }
                
            if (!string.IsNullOrEmpty(aliasIndentifier))
                dataExpr = DataModelExpr.Value.Cast<DataExpr>().ToList()
                    .Single(x => x.AliasIdentifier == aliasIndentifier);

            if (dataExpr == null)
            {
                return mapperLinks
                          .SelectMany(x => x.Resources
                                            .Where(y => y.Source == value)
                                            .SelectMany(x => x.Link))
                           .FirstOrDefault(t => t.Target == target);
            }
            else
            {
                return mapperLinks
                          .Where(x => x.Name == dataExpr.Value)
                          .SelectMany(x => x.Resources
                                            .Where(y => y.Source == value)
                                            .SelectMany(x => x.Link))
                           .FirstOrDefault(t => t.Target == target);
            }
            
        }

    }
}
