using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public abstract class StrategyPart
    {
        #region Expressions

        public DeclareExpr? DeclareExpr { get; set; }

        public PropertiesExpr? PropertiesExpr { get; set; }

        public DataModelExpr? DataModelExpr { get; set; }

        public LinkExpr? LinkExpr { get; set; }

        public FilterExpr? FilterExpr { get; set; }

        public RestrictExpr? RestrictExpr { get; set; }

        public GroupByExpr? GroupByExpr { get; set; }

        public OrderByExpr? OrderByExpr { get; set; }

        public TargetExpr? TargetExpr { get; set; }

        #endregion

        #region Abstract Interfaces

        public abstract string Fetch();

        public abstract string Modify();

        public abstract string Add();

        #endregion

        public string Query(CreatePart request)
        {
            //set expression parts
            DeclareExpr = (DeclareExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(DeclareExpr)));
            PropertiesExpr = (PropertiesExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(PropertiesExpr)));
            DataModelExpr = (DataModelExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(DataModelExpr)));
            LinkExpr = (LinkExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(LinkExpr)));
            FilterExpr = (FilterExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(FilterExpr)));
            RestrictExpr = (RestrictExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(RestrictExpr)));
            GroupByExpr = (GroupByExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(GroupByExpr)));
            OrderByExpr = (OrderByExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(OrderByExpr)));
            TargetExpr = (TargetExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(TargetExpr)));

            //make a copy of incoming expression request
            if (request.Command == Command.FETCH)
                return Fetch();
            else if (request.Command == Command.ADD)
                return Add();
            else if (request.Command == Command.MODIFY)
                return Modify();

            return string.Empty;
        }

        protected Link GetMappedProperty(BaseExpr baseExpr, string database)
        {
            Link link = new Link();
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
            else if (baseExpr is JsonExpr)
            {
                value = ((JsonExpr)baseExpr).Value;
                aliasIndentifier = ((JsonExpr)baseExpr).AliasIdentifier;
            }

            if (DataModelExpr != null)
            {
                if (!string.IsNullOrEmpty(aliasIndentifier))
                    dataExpr = DataModelExpr.Value.Cast<DataExpr>().ToList().Single(x => x.AliasIdentifier == aliasIndentifier);
                else
                    dataExpr = DataModelExpr.Value.Cast<DataExpr>().First();

                var unified = Assistor.USchema.Single(x => x.View.Name == dataExpr.Value);

                if (value.IndexOf('.') > -1)
                {
                    var values = value.Split('.');
                    var count = 0;

                    foreach (var reference in values)
                    {
                        if (count == values.Count() - 1)
                        {
                            link = unified.View.Resources
                                    .Where(x => x.Property == reference)
                                    .SelectMany(x => x.Link.Where(z => z.Target == database)).First();
                            break;
                        }
                        else
                            unified = Assistor.USchema.Single(x => x.View.Name == reference);

                        count++;
                    }
                }
                else
                {
                    link = unified.View.Resources
                        .Where(x => x.Property == value)
                        .SelectMany(x => x.Link.Where(z => z.Target == database)).FirstOrDefault();
                }
            }

            return link;
        }

        protected PropertyPart LeftRightPart(OperatorExpr operatorExpr, DirectionType direction, string database)
        {
            Link? link = null;
            Properties? property = null;

            if (DirectionType.Left == direction)
            {
                link = GetMappedProperty(operatorExpr.Left, database);

                property = Assistor.NSchema
                   .SelectMany(x => x.Model)
                   .Where(x => x.Name == link.Reference)
                   .SelectMany(x => x.Properties)
                   .First(x => x.Property == link.Property);

                return new PropertyPart(link, operatorExpr.Left);
            }
            else
            {
                if (operatorExpr.Right is PropertyExpr || operatorExpr.Right is TermExpr || operatorExpr.Right is JsonExpr)
                {
                    link = GetMappedProperty(operatorExpr.Right, database);

                    property = Assistor.NSchema
                        .SelectMany(x => x.Model)
                        .Where(x => x.Name == link.Reference)
                        .SelectMany(x => x.Properties)
                        .First(x => x.Property == link.Property);

                    return new PropertyPart(link, operatorExpr.Right);
                }
                else
                    return new PropertyPart(operatorExpr.Right);
            }
        }

        protected OrderByPart GetOrderPart(string database)
        {
            OrderByPart part = null;

            var mappedProperty = GetMappedProperty(OrderByExpr, database);

            if (mappedProperty != null)
                part = new OrderByPart(mappedProperty, OrderByExpr);

            return part;
        }

        
    }
}
