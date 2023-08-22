using App.Metrics;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Models.Views.Unified;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Cassandra;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using BaseExpr = CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class CassandraStrategy : StrategyPart
    {
        protected string Target = "cassandra";

        public CassandraStrategy() { }

        public override OutputPart Fetch()
        {
            Console.WriteLine("Starting Cassandra - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToSelectModel();

            if (targetQuery.Count() > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise cassandra query generator
                var generator = new CassandraGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }

            //Console.WriteLine(query);

            return new OutputPart
            {
                Query = query.ToString(),
                Codex = BuildCodex(targetQuery)
            };
        }

        public override OutputPart Modify()
        {
            var output = new OutputPart();

            Console.WriteLine("Starting Cassandra - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToUpdateModel();

            if (targetQuery.Count() > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise cassandra query generator
                var generator = new CassandraGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }

            Console.WriteLine(query);

            output.Query = query.ToString();

            return output;
        }

        public override OutputPart Add()
        {
            var output = new OutputPart();

            Console.WriteLine("Starting Cassandra - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel();

            if (targetQuery.Count() > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise cassandra query generator
                var generator = new CassandraGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }

            Console.WriteLine(query);

            output.Query = query.ToString();

            return output;
        }

        #region Expression Parts

        #region Fetch

        private List<IExpression> ConvertToSelectModel()
        {
            var parts = new List<IExpression>();

            //select
            parts.Add(new SelectPart(GetPropertyPart().ToArray()));

            if (parts.Count > 0)
            {
                //from 
                parts.Add(new FromPart(GetTablePart()));

                if (FilterExpr != null)
                    parts.Add(new ConditionPart(GetLogicalPart()));

                if (OrderByExpr != null)
                    parts.Add(this.GetOrderPart(Target));

                if (RestrictExpr != null)
                    parts.Add(new RestrictPart(RestrictExpr.Value));

                if(FilterExpr != null)
                {
                    if(!DetermineIfIndexed(parts))
                        parts.Add(new AllowFilterPart());
                }
            }

            return parts;
        }

        #endregion

        #region Modify

        private List<IExpression> ConvertToUpdateModel()
        {
            var parts = new List<IExpression>();

            //update 
            parts.Add(new UpdatePart(GetTablePart()));

            if (parts.Count > 0)
            {
                //set values
                parts.AddRange(SetValuePart());

                if (FilterExpr != null)
                    parts.Add(new ConditionPart(GetLogicalPart()));
            }

            return parts;
        }

        #endregion

        #region Add

        private List<IExpression> ConvertToAddModel()
        {
            List<IExpression> parts = new List<IExpression>();

            //insert 
            parts.Add(new InsertPart(GetTablePart()));

            //set values
            if (parts.Count > 0)
                parts.Add(new ValuesPart(InsertValuePart()));

            return parts;
        }

        #endregion

        #endregion

        private IExpression[] GetPropertyPart()
        {
            var parts = new List<IExpression>();

            if(DeclareExpr != null)
            {
                foreach (var part in DeclareExpr.Value)
                {
                    if (part is not FunctionExpr)
                    {
                        dynamic baseExpr = part is PropertyExpr ? ((PropertyExpr)part) : ((JsonExpr)part);

                        var mappedProperty = GetMappedProperty(baseExpr, Target);

                        if (mappedProperty != null && mappedProperty.Link != null)
                        {
                            parts.Add(new PropertyPart(mappedProperty));
                            parts.Add(new SeparatorPart(","));
                        }
                    }
                    else if (part is FunctionExpr)
                    {
                        var expr = (FunctionExpr)part;

                        foreach (var func in expr.Value)
                        {
                            dynamic @base = func is PropertyExpr ? ((PropertyExpr)func) : ((JsonExpr)func);

                            var mappedProperty = GetMappedProperty(@base, Target);

                            if (mappedProperty != null && mappedProperty.Link != null)
                            {
                                mappedProperty.AggregateType = expr.Type;

                                parts.Add(
                                    new NativeFunctionPart(
                                        new PropertyPart(mappedProperty), expr.Type)
                                    );

                                parts.Add(new SeparatorPart(","));
                            }
                        }
                    }
                }

                if (parts.Count > 0)
                    parts.RemoveAt(parts.Count - 1);
            }

            return parts.ToArray();
        }

        private IExpression[] GetTablePart()
        {
            var parts = new List<IExpression>();

            if(DataModelExpr != null)
            {
                foreach (var part in DataModelExpr.Value)
                {
                    if (part is DataExpr)
                    {
                        var data = (DataExpr)part;
                        
                        var link = Assistor.USchema
                            .Select(x => x.View)
                            .Where(x => x.Name == data.Value)
                            .SelectMany(x => x.Resources
                                              .SelectMany(x => x.Link))
                            .Where(t => t.Target == Target)
                            .FirstOrDefault();

                        if (link != null)
                            parts.Add(new TablePart(link.Reference, "", data.Value));
                    }
                }
            }
            

            return parts.ToArray();
        }

        private LogicalPart[] GetLogicalPart()
        {
            var parts = new List<LogicalPart>();

            if (FilterExpr != null)
            {
                foreach (var part in FilterExpr.Value)
                {
                    if (part is GroupExpr)
                    {
                        var groupExpr = (GroupExpr)part;
                        var operatorExpr = (OperatorExpr)groupExpr.Value;

                        var operatorPart = new OperatorPart(operatorExpr.Operator, Database.CASSANDRA);
                        var comparePart = new ComparePart(operatorExpr.Compare, Database.CASSANDRA);

                        var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);
                        var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                        parts.Add(new LogicalPart(leftPart, operatorPart, rightPart, comparePart));
                    }
                }
            }

            return parts.ToArray();
        }

        private IExpression[] SetValuePart()
        {
            var parts = new List<IExpression>();

            if (PropertiesExpr != null)
            {
                foreach (var groups in PropertiesExpr.Value)
                {
                    var exprs = new List<IExpression>();

                    foreach (var part in ((GroupPropertiesExpr)groups).Value)
                    {
                        if (part is GroupExpr)
                        {
                            var groupExpr = (GroupExpr)part;
                            var operatorExpr = (OperatorExpr)groupExpr.Value;

                            var operatorPart = new OperatorPart(operatorExpr.Operator, Database.CASSANDRA);

                            var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);
                            var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                            exprs.Add(new SetValuePart(leftPart, operatorPart, rightPart));
                            exprs.Add(new SeparatorPart(","));
                        }
                    }

                    if (exprs.Count > 0)
                        exprs.RemoveAt(exprs.Count - 1);

                    parts.Add(new SetPart(exprs.ToArray()));
                    parts.Add(new SeparatorPart(","));
                }
            }

            if (parts.Count > 0)
                parts.RemoveAt(parts.Count - 1);

            return parts.ToArray();
        }

        private IExpression[] InsertValuePart()
        {
            var parts = new List<IExpression>();

            if (PropertiesExpr != null)
            {
                foreach (var groups in PropertiesExpr.Value)
                {
                    var leftExprs = new List<IExpression>();
                    var rightExprs = new List<IExpression>();

                    foreach (var part in ((GroupPropertiesExpr)groups).Value)
                    {
                        if (part is GroupExpr)
                        {
                            var groupExpr = (GroupExpr)part;
                            var operatorExpr = (OperatorExpr)groupExpr.Value;

                            var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);
                            var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                            if (leftPart != null && rightPart != null)
                            {
                                leftExprs.Add(leftPart);
                                leftExprs.Add(new SeparatorPart(","));

                                rightExprs.Add(rightPart);
                                rightExprs.Add(new SeparatorPart(","));
                            }
                        }
                    }

                    if (leftExprs.Count > 0)
                        leftExprs.RemoveAt(leftExprs.Count - 1);

                    if (rightExprs.Count > 0)
                        rightExprs.RemoveAt(rightExprs.Count - 1);

                    parts.Add(new InsertValuePart(leftExprs.ToArray(), rightExprs.ToArray()));
                    parts.Add(new SeparatorPart(","));
                }
            }

            if (parts.Count > 0)
                parts.RemoveAt(parts.Count - 1);

            return parts.ToArray();
        }

        private LinkedProperty GetMappedProperty(BaseExpr.BaseExpr baseExpr, string database)
        {
            LinkedProperty mappedProperty = new LinkedProperty();

            dynamic? expr = baseExpr is PropertyExpr ? ((PropertyExpr)baseExpr) :
                            baseExpr is TermExpr ? ((TermExpr)baseExpr) :
                            baseExpr is OrderByPropertyExpr ? ((OrderByPropertyExpr)baseExpr) :
                            baseExpr is JsonExpr ? ((JsonExpr)baseExpr) : default;

            if (expr != null)
            {
                mappedProperty.Type = baseExpr.GetType();
                mappedProperty.Property = expr.Value;
                mappedProperty.AliasIdentifier = expr.AliasIdentifier;

                //if (expr.GetType().GetProperty("AliasName") != null)
                //    mappedProperty.AliasName = expr.AliasName;

                if (DataModelExpr != null)
                {
                    DataExpr dataExpr;

                    if (!string.IsNullOrEmpty(mappedProperty.AliasIdentifier))
                        dataExpr = DataModelExpr.Value.Cast<DataExpr>().ToList().Single(x => x.AliasIdentifier == mappedProperty.AliasIdentifier);
                    else
                        dataExpr = DataModelExpr.Value.Cast<DataExpr>().First();

                    if (mappedProperty.Property.IndexOf('.') > -1)
                    {
                        var iterator = 0;

                        Resources resource = null;
                        Resources prevresource = null;

                        mappedProperty.Link = new Link();

                        var @base = Assistor.USchema.Single(x => x.View.Name == dataExpr.Value);

                        var target = (Database)Enum.Parse(typeof(Database), database.ToUpper());

                        //get link references based on the data model
                        var references = @base.View.Resources
                                        .Where(x => x.Property == "base")
                                        .SelectMany(x => x.Link.Where(x => x.Target == database))
                                        .First();

                        //get all linked properties, model, etc
                        var native = Assistor.NSchema[(int)target]
                            .SelectMany(s => s.Model)
                            .Where(x => x.Name == references.Reference)
                            .FirstOrDefault();

                        foreach (var reference in mappedProperty.Property.Split('.'))
                        {
                            if (iterator == 0)
                            {
                                //set base reference i.e first reference in json path
                                resource = @base.View.Resources.Single(x => x.Property == reference);
                            }
                            else
                            {
                                @base = Assistor.USchema.Single(x => x.View.Name == prevresource?.Type);

                                resource = @base.View.Resources.Single(x => x.Property == reference);
                            }

                            if (resource.Metadata != "class")
                            {
                                var link = SearchAndFindPropertyLink(prevresource?.Property, reference, database);

                                if (link != null)
                                {
                                    var field = Assistor.NSchema[(int)target]
                                        .SelectMany(x => x.Model.Where(x => x.Name == link.Reference))
                                        .SelectMany(x => x.Properties.Where(x => x.Property == link.Property))
                                        .First();

                                    //find root 
                                    var path = GetPropertyPath(native.Name, link.Reference, target);

                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        if (path != dataExpr.Value)
                                            mappedProperty.Link.Property = path + "." + field.Property;
                                        else
                                            mappedProperty.Link.Property = field.Property;
                                    }
                                    else
                                        mappedProperty.Link.Property = field.Property;

                                    mappedProperty.Link.Reference = link.Reference;
                                    mappedProperty.Link.Target = link.Target;
                                    mappedProperty.SourceReference = @base.View.Name;

                                }
                                else
                                    mappedProperty.Link = default;
                            }

                            prevresource = resource;
                            iterator++;
                        }
                    }
                    else
                    {
                        mappedProperty.Link = SearchAndFindPropertyLink(dataExpr.Value, mappedProperty.Property, database);
                        mappedProperty.SourceReference = dataExpr.Value;
                    }

                }
            }

            return mappedProperty;
        }

        private PropertyPart? LeftRightPart(OperatorExpr operatorExpr, DirectionType direction, string database)
        {
            if (DirectionType.Left == direction)
            {
                var left = GetMappedProperty(operatorExpr.Left, database);

                if (left.Link != null)
                    return new PropertyPart(left);
            }

            else
            {
                if (operatorExpr.Right is PropertyExpr || operatorExpr.Right is TermExpr || operatorExpr.Right is JsonExpr)
                {
                    var right = GetMappedProperty(operatorExpr.Right, database);

                    if (right.Link != null)
                        return new PropertyPart(right);
                }
                else
                    return new PropertyPart(operatorExpr.Right);
            }

            return default;
        }

        private OrderByPart GetOrderPart(string database)
        {
            OrderByPart part = null;

            if (OrderByExpr != null)
            {
                var parts = new List<IExpression>();

                foreach (var expr in OrderByExpr.Properties)
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

        private string GetPropertyPath(string @base, string propertyReference, Database target)
        {
            var path = string.Empty;

            var models = Assistor.NSchema[(int)target]
                                      .SelectMany(x => x.Model)
                                      .Where(x => x.Properties.Exists(t => t.Property == propertyReference))
                                      .FirstOrDefault();
            //x.Name == modelReference &&
            //string modelReference,
            if (models != null)
            {
                if (@base == models.Name)
                    path = propertyReference;
                else
                {
                    var test = GetPropertyPath(@base, models.Name, target);

                    if (string.IsNullOrEmpty(path))
                    {
                        if (string.IsNullOrEmpty(test))
                            path = models.Name + "." + propertyReference;
                        else
                            path = test + "." + propertyReference;
                    }

                    else
                        path = path + "." + test;
                }
            }

            return path;

        }

        private bool DetermineIfIndexed(List<IExpression> parts)
        {
            var properties = parts
                        .Where(x => x.GetType().Equals(typeof(ConditionPart)))
                        .Select(x => (ConditionPart)x)
                        .SelectMany(x => x.Logic
                                            .Where(y => y.GetType().Equals(typeof(LogicalPart)))
                                            .Select(y => (LogicalPart)y))
                        .Where(x => x.Left.GetType().Equals(typeof(PropertyPart)))
                        .Select(x => x.Left).ToList();

            var clusteredIndexes = Assistor.NSchema[(int)Database.CASSANDRA]
                             .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                             .Where(x => x.Key)
                             .Select(x => x.Property).ToList();

            var nonClusteredIndexes = Assistor.NSchema[(int)Database.CASSANDRA]
                             .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                             .Where(x => (x.Indexed))
                             .Select(x => x.Property).ToList();

           
            if (properties != null && properties.Count() > 0)
            {
                var count = clusteredIndexes.Count(x => properties.Select(x => x.Name).Contains(x));

                if (count == clusteredIndexes.Count)
                    return true;
                else
                {
                    count = nonClusteredIndexes.Count(x => properties.Select(x => x.Name).Contains(x));

                    if (count > 1)
                        return true;
                }
            }

            return default;
        }

        private Codex BuildCodex(List<IExpression> parts)
        {
            FromProperty from;
            ToProperty to;

            var codex = new Codex
            {
                Target = Database.CASSANDRA,
                PropertyModel = new List<Model>(),
                DataModel = DataModelExpr
            };

            var fromPart = (FromPart?)parts.SingleOrDefault(x => x.GetType().Equals(typeof(FromPart)));

            if(fromPart != null)
            {
                foreach (TablePart table in fromPart.Properties.Where(x => x.GetType().Equals(typeof(TablePart))))
                {
                    var model = new Model
                    {
                        Name = table.Source,
                        Views = new Dictionary<FromProperty, ToProperty>()
                    };

                    var selectPart = (SelectPart?)parts.SingleOrDefault(x => x.GetType().Equals(typeof(SelectPart)));

                    if(selectPart != null)
                    {
                        foreach (var select in selectPart.Properties)
                        {
                            if(select is PropertyPart)
                            {
                                var property = (PropertyPart)select;

                                from = new FromProperty
                                {
                                    Name = property.Name,
                                    Alias = property.AliasName
                                };

                                to = new ToProperty
                                {
                                    Name = property.Source
                                };

                                model.Views.Add(from, to);
                            }
                            else if (select is NativeFunctionPart)
                            {
                                var function = (NativeFunctionPart)select;

                                if(function.Property is PropertyPart)
                                {
                                    var property = (PropertyPart)function.Property;

                                    from = new FromProperty
                                    {
                                        Name = property.Name,
                                        Alias = property.AliasName
                                    };

                                    to = new ToProperty
                                    {
                                        Name = property.Source
                                    };

                                    model.Views.Add(from, to);
                                }
                            }
                        }
                    }

                    codex.PropertyModel.Add(model);
                }
            }
            
            return codex;
        }
    }
}
