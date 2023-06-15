using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
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

        public FilterExpr? FilterExpr { get; set; }

        public RestrictExpr? RestrictExpr { get; set; }

        public OrderByExpr? OrderByExpr { get; set; }

        public TargetExpr? TargetExpr { get; set; }

        #endregion

        protected bool DoesQueryContainFunction
        {
            get
            {
                if (DeclareExpr != null)
                {
                    dynamic? expr = DeclareExpr.Value.FirstOrDefault(x => x.GetType().Equals(typeof(FunctionExpr))) as FunctionExpr;

                    if (expr != null)
                    {
                        if (expr.Type == AggregateType.NSum || expr.Type == AggregateType.NAvg ||
                            expr.Type == AggregateType.NMin || expr.Type == AggregateType.NMax || expr.Type == AggregateType.NCount)
                            return true;
                    }
                }
                return false;
            }
        }


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
            FilterExpr = (FilterExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(FilterExpr)));
            RestrictExpr = (RestrictExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(RestrictExpr)));
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

        protected string[]? GetReferencesBasedOnDeclare(string target)
        {
            List<string>? references = null;

            if (DeclareExpr != null)
            {
                references = new List<string>();

                //get json expressions
                var properties = GetDeclare<PropertyExpr>();

                if(properties != null)
                {
                    foreach (var property in properties)
                    {
                        var mappedProperty = GetMappedProperty(property, target);

                        if (mappedProperty.Link != null)
                            if (!references.Contains(mappedProperty.Link.Reference))
                                references.Add(mappedProperty.Link.Reference);
                    }
                }

                //get json expressions
                var jsons = GetDeclare<JsonExpr>();
                
                if(jsons != null)
                {
                    foreach (var json in jsons)
                    {
                        var mappedProperty = GetMappedProperty(json, target);

                        if (mappedProperty.Link != null)
                            if (!references.Contains(mappedProperty.Link.Reference))
                                references.Add(mappedProperty.Link.Reference);
                    }
                }

                return references.ToArray();
            }

            return default;
        }

        protected string[]? GetReferencesBasedOnProperties(string target)
        {
            List<string>? references = null;

            if (PropertiesExpr != null)
            {
                references = new List<string>();

                //get json expressions
                var properties = GetProperties<TermExpr>();

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        var mappedProperty = GetMappedProperty(property, target);

                        if (mappedProperty.Link != null)
                            if (!references.Contains(mappedProperty.Link.Reference))
                                references.Add(mappedProperty.Link.Reference);
                    }
                }

                return references.ToArray();
            }

            return default;
        }

        protected LinkedProperty GetMappedProperty(BaseExpr baseExpr, string database)
        {
            LinkedProperty mappedProperty = new LinkedProperty();

            dynamic? expr = baseExpr is PropertyExpr ? ((PropertyExpr)baseExpr) :
                            baseExpr is TermExpr ? ((TermExpr)baseExpr) :
                            baseExpr is OrderByPropertyExpr ? ((OrderByPropertyExpr)baseExpr) :
                            baseExpr is JsonExpr ? ((JsonExpr)baseExpr) : default;

            if(expr != null)
            {
                mappedProperty.Type = baseExpr.GetType();
                mappedProperty.Property = expr.Value;
                mappedProperty.AliasIdentifier = expr.AliasIdentifier;

                if (expr.GetType().GetProperty("AliasName") != null)
                    mappedProperty.AliasName =  expr.AliasName;

                if (DataModelExpr != null)
                {
                    DataExpr dataExpr;

                    if (!string.IsNullOrEmpty(mappedProperty.AliasIdentifier))
                        dataExpr = DataModelExpr.Value.Cast<DataExpr>().ToList().Single(x => x.AliasIdentifier == mappedProperty.AliasIdentifier);
                    else
                        dataExpr = DataModelExpr.Value.Cast<DataExpr>().First();

                    var uschema = Assistor.USchema.Single(x => x.View.Name == dataExpr.Value);

                    if (mappedProperty.Property.IndexOf('.') > -1)
                    {
                        //convert json path to an array
                        var links = ConvertJsonToLinks(mappedProperty.Property.Split('.'), database);
                        var count = 0;

                        mappedProperty.Link = new Link();

                        foreach (var link in links.Where(x => x != null))
                        {
                            if(count == 0)
                            {
                                mappedProperty.Link.Target = database;
                                mappedProperty.Link.Reference = link.Reference;
                            }

                            mappedProperty.Link.Property = string.IsNullOrEmpty(mappedProperty.Link.Property) 
                                ? link.Property 
                                : mappedProperty.Link.Property + "." + link.Property;

                            count++;
                        }
                    }
                    else
                        mappedProperty.Link = uschema.View.Resources
                            .Where(x => x.Property == mappedProperty.Property)
                            .SelectMany(x => x.Link.Where(z => z.Target == database)).FirstOrDefault();
                }
            }

            return mappedProperty;
        }
        
        protected PropertyPart LeftRightPart(OperatorExpr operatorExpr, DirectionType direction, string database, int target)
        {
            if (DirectionType.Left == direction)
                return new PropertyPart(GetMappedProperty(operatorExpr.Left, database));
            else
            {
                if (operatorExpr.Right is PropertyExpr || operatorExpr.Right is TermExpr || operatorExpr.Right is JsonExpr)
                    return new PropertyPart(GetMappedProperty(operatorExpr.Right, database));
                else
                    return new PropertyPart(operatorExpr.Right);
            }
        }

        protected OrderByPart GetOrderPart(string database)
        {
            OrderByPart part = null;

            if(OrderByExpr != null)
            {
                var parts = new List<IExpression>();

                foreach(var expr in OrderByExpr.Properties)
                {
                    var mappedProperty = GetMappedProperty((OrderByPropertyExpr)expr, database);

                    if (mappedProperty != null)
                    {
                        parts.Add(new OrderByPropertyPart(mappedProperty.Link, (OrderByPropertyExpr)expr));
                        parts.Add(new SeparatorPart(","));
                    }
                }

                if (parts.Count > 0)
                    parts.RemoveAt(parts.Count - 1);

                part = new OrderByPart(parts.ToArray());
            }
            
            return part;
        }

        private Link[] ConvertJsonToLinks(string[] values, string database)
        {
            var links = new List<Link>();

            //set base reference i.e first reference in json path
            var @base = values[0];
            var count = 0;
            var prevReference = string.Empty;
            
            foreach (var reference in values.Where(x => x != @base))
            {
                var link = new Link();

                if (count == 0)
                    link = SearchAndFindPropertyLink(@base, reference, database);
                else
                    link = SearchAndFindPropertyLink(prevReference, reference, database);

                if (link != null)
                    links.Add(link);

                prevReference = reference;
                count++;
            }

            return links.ToArray();
        }

        private Link? SearchAndFindPropertyLink(string model, string property, string database)
        {
            return Assistor.USchema
                        .Where(x => x.View.Name == model)
                        .SelectMany(x => x.View.Resources)
                        .Where(x => x.Property == property)
                        .SelectMany(x => x.Link)
                        .Where(x => x.Target == database)
                        .FirstOrDefault();
        }

        private T[]? GetDeclare<T>()
        {
            return DeclareExpr?.Value
                    .Where(x => x.GetType().Equals(typeof(T)))
                    .Select(x => (T)Convert.ChangeType(x, typeof(T)))
                    .Union(
                        DeclareExpr.Value
                        .Where(x => x.GetType().Equals(typeof(FunctionExpr)))
                        .Select(x => (FunctionExpr)x)
                        .SelectMany(x => x.Value.Where(x => x.GetType().Equals(typeof(T)))
                                    .Select(x => (T)Convert.ChangeType(x, typeof(T)))))
                    .ToArray();
        }

        private T[]? GetProperties<T>()
        {
            var groups = PropertiesExpr?.Value
                                    .Where(x => x.GetType().Equals(typeof(GroupPropertiesExpr)))
                                    .Select(x => (GroupPropertiesExpr)x)
                                    .SelectMany(x => x.Value
                                                       .Where(x => x.GetType().Equals(typeof(GroupExpr)))
                                                       .Select(x => (GroupExpr)x));
            
            if (groups != null)
            {
                return groups.Where(x => x.Value.GetType().Equals(typeof(OperatorExpr)))
                             .Select(x => (OperatorExpr)x.Value)
                             .Where(x => x.Left.GetType().Equals(typeof(T)))
                             .Select(x => (T)Convert.ChangeType(x.Left, typeof(T)))
                             .Union(
                                    groups.Where(x => x.Value.GetType().Equals(typeof(OperatorExpr)))
                                             .Select(x => (OperatorExpr)x.Value)
                                             .Where(x => x.Right.GetType().Equals(typeof(T)))
                                             .Select(x => (T)Convert.ChangeType(x.Right, typeof(T))))
                             .ToArray();
            }

            return default;
        }
    }
}
