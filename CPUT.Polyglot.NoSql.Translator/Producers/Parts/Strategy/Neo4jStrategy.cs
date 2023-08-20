using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Models.Views.Unified;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Cassandra;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using MongoDB.Driver.Linq;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using Model = CPUT.Polyglot.NoSql.Models.Translator.Executors.Model;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class Neo4jStrategy : StrategyPart
    {
        protected string Target = "neo4j";

        #region private variables

        private IExpression[]? _unwindParts { get; set; }
        public IExpression[] UnwindParts
        {
            get
            {
                if (_unwindParts == null)
                    _unwindParts = GetUnwindPart();

                return _unwindParts;
            }
        }

        #endregion

        public override OutputPart Fetch()
        {
            Console.WriteLine("Starting Neo4jPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToSelectModel();

            if (targetQuery.Count > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise neo4j query generator
                var generator = new Neo4jGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }

            return new OutputPart
            {
                Query = query.ToString(),
                Codex = BuildCodex(targetQuery)
            };
        }

        public override OutputPart Modify()
        {
            var output = new OutputPart();

            Console.WriteLine("Starting Neo4jPart - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToUpdateModel();

            if (targetQuery.Count > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise cassandra query generator
                var generator = new Neo4jGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }

            output.Query = query.ToString();

            return output;
        }

        public override OutputPart Add()
        {
            var output = new OutputPart();

            Console.WriteLine("Starting Neo4jPart - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel();

            if (targetQuery.Count > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise cassandra query generator
                var generator = new Neo4jGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }
            
            output.Query = query.ToString();

            return output;
        }

        #region Expression Parts

        #region Select
        
        public List<IExpression> ConvertToSelectModel()
        {
            var parts = new List<IExpression>();

            //match
            parts.Add(new MatchPart(GetNodePart()));

            if (parts.Count > 0)
            {
                //get unwind fields
                parts.AddRange(UnwindParts);

                //get property fields
                var properties = GetPropertyPart();

                if (this.FilterExpr != null)
                {
                    if (parts.Count > 1)
                        parts.Add(new WithPart(GetWithParts(properties.ToArray())));

                    parts.Add(new ConditionPart(GetLogicalPart().ToArray()));
                }

                //get property fields
                parts.Add(new ReturnPart(properties.ToArray()));

                if (OrderByExpr != null)
                    parts.Add(GetOrderPart(Target));

                if (this.RestrictExpr != null)
                    parts.Add(new RestrictPart(this.RestrictExpr.Value));
            }

            return parts;
        }

        #endregion

        #region Update

        public List<IExpression> ConvertToUpdateModel()
        {
            var parts = new List<IExpression>();

            parts.Add(new MatchPart(GetNodePart().ToArray()));

            if (parts.Count > 0)
            {
                if (this.FilterExpr != null)
                    parts.Add(new ConditionPart(GetLogicalPart().ToArray()));

                parts.AddRange(GetInsertValuePart(false).ToArray());

                if (OrderByExpr != null)
                    parts.Add(GetOrderPart(Target));

                if (this.RestrictExpr != null)
                    parts.Add(new RestrictPart(this.RestrictExpr.Value));
            }

            return parts;
        }

        #endregion

        #region Add

        public List<IExpression> ConvertToAddModel()
        {
            var parts = new List<IExpression>();

            var innerParts = new List<IExpression>();
            
            innerParts.AddRange(GetNodePart().ToArray());

            if (innerParts.Count > 0)
            {
                //set values
                innerParts.AddRange(GetInsertValuePart(true).ToArray());

                parts.Add(new InsertPart(innerParts.ToArray()));
            }

            return parts;
        }

        #endregion

        private NodePart[] GetNodePart()
        {
            var parts = new List<NodePart>();

            if (DataModelExpr != null)
            {
                Dictionary<string, string> references = null;


                //get references
                if (DeclareExpr != null)
                    references = GetReferencesBasedOnDeclare(Target);
                else if (PropertiesExpr != null)
                    references = GetReferencesBasedOnProperties(Target);


                if (references != null)
                {
                    foreach (var reference in references)
                    {
                        //get model
                        var model = Assistor.USchema.Single(x => x.View.Name == reference.Value);
                        //get link references based on the data model
                        var link = model.View.Resources
                                        .SelectMany(x => x.Link.Where(x => x.Target == Enum.GetName(typeof(Database), Database.NEO4J).ToLower()))
                                        .FirstOrDefault(x => x.Reference == reference.Key);

                        if (link != null)
                        {
                            var native = Assistor.NSchema[(int)Database.NEO4J]
                                        .SelectMany(s => s.Model)
                                        .Where(x => x.Name == reference.Key && x.Type == "node")
                                .First();

                            parts.Add(new NodePart(native.Name, reference.Value, native.Relations?.ToArray()));
                        }
                    }
                    
                }
            }

            return parts.ToArray();
        }

        private IExpression[] GetWithParts(IExpression[] properties)
        {
            var parts = new List<IExpression>();
          
            if (properties != null)
            {
                foreach (var part in properties)
                {
                    if (part is PropertyPart)
                    {
                        var property = (PropertyPart)part;

                        var aliases = parts.Where(x => x.GetType().Equals(typeof(WithAliasPart))).Select(x => (WithAliasPart)x).ToList();

                        if (!aliases.Exists(x => ((WithAliasPart)x).Value == property.AliasIdentifier))
                        {
                            parts.Add(new WithAliasPart(property.AliasIdentifier));
                            parts.Add(new SeparatorPart(","));
                        }
                    }
                    else if (part is NativeFunctionPart)
                    {
                        var function = (NativeFunctionPart)part;

                        var property = (PropertyPart)function.Property;

                        var aliases = parts.Where(x => x.GetType().Equals(typeof(WithAliasPart))).Select(x => (WithAliasPart)x).ToList();

                        if (!aliases.Exists(x => ((WithAliasPart)x).Value == property.AliasIdentifier))
                        {
                            parts.Add(new WithAliasPart(property.AliasIdentifier));
                            parts.Add(new SeparatorPart(","));
                        }
                    }
                }
            }

            if (parts.Count > 0)
                parts.RemoveAt(parts.Count - 1);

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

                        var operatorPart = new OperatorPart(operatorExpr.Operator, Common.Helpers.Utils.Database.NEO4J);
                        var comparePart = new ComparePart(operatorExpr.Compare, Common.Helpers.Utils.Database.NEO4J);

                        var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);
                        var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                        parts.Add(new LogicalPart(leftPart, operatorPart, rightPart, comparePart));
                    }
                }
            }

            return parts.ToArray();
        }

        private IExpression[] GetPropertyPart()
        {
            var parts = new List<IExpression>();

            if(DeclareExpr != null)
            {
                foreach (var part in DeclareExpr.Value)
                {
                    if (part is not FunctionExpr)
                    {
                        dynamic expr = part is PropertyExpr ? (PropertyExpr)part : (JsonExpr)part;

                        var mappedProperty = GetMappedProperty(expr, Target);

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
                            dynamic nestedExpr = func is PropertyExpr ? ((PropertyExpr)func) : ((JsonExpr)func);

                            var mappedProperty = GetMappedProperty(nestedExpr, Target);

                            if (mappedProperty != null && mappedProperty.Link != null)
                            {
                                parts.Add(new NativeFunctionPart(
                                        new PropertyPart(mappedProperty), expr.Type)
                                    );
                                parts.Add(new SeparatorPart(","));
                            }
                        }
                    }
                }
            }

            if (parts.Count > 0)
                parts.RemoveAt(parts.Count - 1);

            return parts.ToArray();
        }

        private IExpression[] GetInsertValuePart(bool add)
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

                            var operatorPart = new OperatorPart(operatorExpr.Operator, Common.Helpers.Utils.Database.NEO4J);

                            var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);
                            var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                            if (add)
                                exprs.Add(new InsertNodePart(leftPart, operatorPart, rightPart));
                            else
                                exprs.Add(new SetValuePart(leftPart, operatorPart, rightPart));

                            exprs.Add(new SeparatorPart(","));
                        }
                    }

                    if (exprs.Count > 0)
                        exprs.RemoveAt(exprs.Count - 1);

                    if (add)
                        parts.Add(new ValuesPart(exprs.ToArray()));
                    else
                        parts.Add(new SetPart(exprs.ToArray()));

                    parts.Add(new SeparatorPart(","));
                }
            }

            if (parts.Count > 0)
                parts.RemoveAt(parts.Count - 1);

            return parts.ToArray();
        }

        #endregion

        private IExpression[] GetUnwindPart()
        {
            var parts = new List<IExpression>();

            if (DeclareExpr != null)
            {
                var graphs = new List<IExpression>();

                foreach (var expr in DeclareExpr.Value)
                {
                    if (expr is not FunctionExpr)
                    {
                        var mappedProperty = GetMappedProperty(expr, Target);

                        if (mappedProperty.Link != null)
                        {
                            var reference = string.Empty;

                            if (mappedProperty.Link.Property.IndexOf(".") > 0)
                                reference = mappedProperty.Link.Property.Split(".")[0];
                            else
                                reference = mappedProperty.Link.Reference;

                            var model = Assistor.NSchema[(int)Database.NEO4J].SelectMany(x => x.Model.Where(x => x.Name == reference)).FirstOrDefault();

                            if (model != null)
                            {
                                if (model.Type == "array")
                                {
                                    var graph = new UnwindGraphPart(mappedProperty);

                                    if (graphs
                                        .Where(x => x.GetType().Equals(typeof(UnwindGraphPart)))
                                        .Count(x => ((UnwindGraphPart)x).UnwindProperty == graph.UnwindProperty) < 1)
                                    {
                                        graphs.Add(graph);
                                        graphs.Add(new SeparatorPart(","));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var funcExpr in ((FunctionExpr)expr).Value)
                        {
                            var mappedProperty = GetMappedProperty(funcExpr, Target);

                            if (mappedProperty.Link != null)
                            {
                                var reference = string.Empty;

                                if (mappedProperty.Link.Property.IndexOf(".") > 0)
                                    reference = mappedProperty.Link.Property.Split(".")[0];
                                else
                                    reference = mappedProperty.Link.Reference;

                                var model = Assistor.NSchema[(int)Database.NEO4J].SelectMany(x => x.Model.Where(x => x.Name == reference)).FirstOrDefault();

                                if (model != null)
                                {
                                    if (model.Type == "array")
                                    {
                                        var graph = new UnwindGraphPart(mappedProperty);

                                        if (graphs
                                            .Where(x => x.GetType().Equals(typeof(UnwindGraphPart)))
                                            .Count(x => ((UnwindGraphPart)x).UnwindProperty == graph.UnwindProperty) < 1)
                                        {
                                            graphs.Add(graph);
                                            graphs.Add(new SeparatorPart(","));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (graphs.Count > 0)
                {
                    graphs.RemoveAt(graphs.Count - 1);
                    parts.Add(new UnwindGroupPart(graphs.ToArray()));
                }
            }

            return parts.ToArray();
        }

        private Codex BuildCodex(List<IExpression> parts)
        {
            FromProperty from;
            ToProperty to;

            var codex = new Codex
            {
                Target = Database.NEO4J,
                PropertyModel = new List<Model>(),
                DataModel = DataModelExpr
            };

            var matchPart = (MatchPart?)parts.SingleOrDefault(x => x.GetType().Equals(typeof(MatchPart)));

            if (matchPart != null)
            {
                foreach (NodePart nodePart in matchPart.Properties.Where(x => x.GetType().Equals(typeof(NodePart))))
                {
                    var model = new Model
                    {
                        Name = nodePart.Source,
                        Views = new Dictionary<FromProperty, ToProperty>()
                    };

                    var returnPart = (ReturnPart?)parts.SingleOrDefault(x => x.GetType().Equals(typeof(ReturnPart)));

                    if (returnPart != null)
                    {
                        foreach (var select in returnPart.Properties)
                        {
                            if (select is PropertyPart)
                            {
                                var property = (PropertyPart)select;

                                from = new FromProperty
                                {
                                    Name = property.Name,
                                    Alias = property.AliasIdentifier
                                };

                                to = new ToProperty
                                {
                                    Name = property.Source,
                                };

                                model.Views.Add(from, to);
                            }
                            else if (select is NativeFunctionPart)
                            {
                                var function = (NativeFunctionPart)select;

                                if (function.Property is PropertyPart)
                                {
                                    var property = (PropertyPart)function.Property;

                                    from = new FromProperty
                                    {
                                        Name = property.Name,
                                    };

                                    to = new ToProperty
                                    {
                                        Name = property.Source,
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

        protected Dictionary<string, string> GetReferencesBasedOnDeclare(string target)
        {
            var references = new Dictionary<string, string>();

            if (DeclareExpr != null)
            {
                //get json expressions
                var properties = GetDeclare<PropertyExpr>();

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        var mappedProperty = GetMappedProperty(property, target);

                        if (mappedProperty.Link != null)
                        {
                            if (!references.ContainsKey(mappedProperty.Link.Reference))
                                references.Add(mappedProperty.Link.Reference, mappedProperty.SourceReference);
                        }
                    }
                }

                //get json expressions
                var jsons = GetDeclare<JsonExpr>();

                if (jsons != null)
                {
                    foreach (var json in jsons)
                    {
                        var mappedProperty = GetMappedProperty(json, target);

                        if (mappedProperty.Link != null)
                        {
                            var model = Assistor.NSchema[(int)Database.NEO4J].SelectMany(x => x.Model.Where(x => x.Name == mappedProperty.Link.Reference)).First();

                            if (model.Type != "array")
                            {
                                if (!references.ContainsKey(mappedProperty.Link.Reference))
                                    references.Add(mappedProperty.Link.Reference, mappedProperty.SourceReference);
                            }
                            else
                            {
                                var referenceModel = Assistor.NSchema[(int)Database.NEO4J].SelectMany(x => x.Model)
                                    .Where(x => x.Properties.Exists(x => x.Property == model.Name))
                                    .First();

                                if (!references.ContainsKey(referenceModel.Name))
                                    references.Add(referenceModel.Name, mappedProperty.Property.Split(".")[0]);
                            }

                        }
                    }
                }

                return references;
            }

            return default;
        }

        private Dictionary<string, string> GetReferencesBasedOnProperties(string target)
        {
            var references = new Dictionary<string, string>();

            if (PropertiesExpr != null)
            {
                //get json expressions
                var properties = GetProperties<TermExpr>();

                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        var mappedProperty = GetMappedProperty(property, target);

                        if (mappedProperty.Link != null)
                        {
                            if (!references.ContainsKey(mappedProperty.Link.Reference))
                                references.Add(mappedProperty.Link.Reference, mappedProperty.SourceReference);
                        }
                    }
                }

                return references;
            }

            return default;
        }


        private LinkedProperty GetMappedProperty(BaseExpr baseExpr, string database)
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

                if (expr.GetType().GetProperty("AliasName") != null)
                    mappedProperty.AliasName = expr.AliasName;

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

                        var resourcepath = new List<string>();

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

                            resourcepath.Add(resource.Property);

                            if (resource.Metadata != "class")
                            {
                                var link = SearchAndFindPropertyLink(prevresource?.Property, reference, database);

                                if (link != null)
                                {
                                    if (link.Property.IndexOf(".") > 0)
                                    {
                                        var parts = link.Property.Split(".");

                                        var field = Assistor.NSchema[(int)target]
                                            .SelectMany(x => x.Model.Where(x => x.Name == parts[0]))
                                            .SelectMany(x => x.Properties.Where(x => x.Property == parts[1]))
                                            .First();

                                        //find root 
                                        var path = GetPropertyPath(link.Reference, parts[0], target);

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
                                    {
                                        var field = Assistor.NSchema[(int)target]
                                        .SelectMany(x => x.Model.Where(x => x.Name == link.Reference))
                                        .SelectMany(x => x.Properties.Where(x => x.Property == link.Property))
                                        .First();

                                        //find root 
                                        var path = GetPropertyPath(resourcepath[0], link.Reference, target);

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
                                      .Where(x => x.Name == @base && x.Properties.Exists(t => t.Property == propertyReference))
                                      .FirstOrDefault();
            
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