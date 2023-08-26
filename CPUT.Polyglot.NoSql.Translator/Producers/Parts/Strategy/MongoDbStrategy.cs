using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Models.Views.Unified;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using App.Metrics;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class MongoDbStrategy : StrategyPart
    {
        protected string Target = "mongodb";

        protected bool DoAggregateModel
        {
            get
            {
                if (DeclareExpr != null)
                {
                    dynamic? expr = DeclareExpr.Value.FirstOrDefault(x => x.GetType().Equals(typeof(FunctionExpr))) as FunctionExpr;

                    if (expr != null)
                        return DoesQueryContainFunction;
                    else
                    {
                        expr = DeclareExpr.Value.FirstOrDefault(x => x.GetType().Equals(typeof(JsonExpr))) as JsonExpr;

                        if (expr != null)
                            return true;
                    }
                }

                return false;
            }
        }

        public MongoDbStrategy() { }

        public override OutputPart Fetch()
        {
            Console.WriteLine("Starting MongoDbPart - Fetch");

            CollectionPart[] targetQuery;

            //initialise mongodb query generator
            var query = new StringBuilder();

            //set expression parts
            targetQuery = FindOrAggregateModel();

            if (targetQuery.Count() > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery);
               
                var generator = new MongoDbGenerator(query, DoAggregateModel ? MongoDBFetchType.Aggregate : MongoDBFetchType.Find);

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

            Console.WriteLine("Starting MongoDbPart - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToModifyModel();

            if (targetQuery.Count() > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise mongodb query generator
                var generator = new MongoDbGenerator(query, MongoDBFetchType.None);

                //kick off visitors
                match.Accept(generator);
            }

            output.Query = query.ToString();

            return output;
        }

        public override OutputPart Add()
        {
            var output = new OutputPart();

            Console.WriteLine("Starting MongoDbPart - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel();

            if (targetQuery.Count() > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise mongodb query generator
                var generator = new MongoDbGenerator(query, MongoDBFetchType.None);

                //kick off visitors
                match.Accept(generator);
            }
            
            output.Query = query.ToString();

            return output;
        }

        #region Expression Parts

        #region Find

        private CollectionPart[] FindOrAggregateModel()
        {
            if (DoAggregateModel)
                return CreateAggregateModel();
            else
                return CreateFindModel();
        }

        private CollectionPart[] CreateFindModel()
        {
            //get collection(s)
            var collections = GetCollectionPart();

            if (collections.Count() > 0)
            {
                foreach (var col in collections)
                {
                    col.Find = new FindPart();

                    //get property fields
                    col.Find.Field = new FieldPart(GetPropertyPart().ToArray());
                    col.Find.CollectionName = col.Target;

                    //get conditions
                    if (FilterExpr != null)
                        col.Find.Condition = new ConditionPart(GetLogicalPart().ToArray());

                    if (OrderByExpr != null)
                        col.Find.OrderBy = GetOrderPart(Target);

                    if (this.RestrictExpr != null)
                        col.Find.Restrict = new RestrictPart(this.RestrictExpr.Value);
                }
            }

            return collections;
        }

        private CollectionPart[] CreateAggregateModel()
        {
            //get collection(s)
            var collections = GetCollectionPart();

            if (collections.Count() > 0)
            {
                foreach (var col in collections)
                {
                    col.Aggregate = new AggregatePart();
                    col.Aggregate.CollectionName = col.Target;

                    //apply conditions
                    if (FilterExpr != null)
                    {
                        col.Aggregate.Match = new MatchPart(
                            new List<IExpression> {
                                new ConditionPart(GetLogicalPart())
                            }.ToArray());
                    }

                    if (DeclareExpr != null)
                    {
                        //unwind complex field i.e. array
                        var unwindParts = GetUnwindPart(Target).ToArray();

                        if (unwindParts.Length > 0)
                            col.Aggregate.Unwind = new UnwindGroupPart(GetUnwindPart(Target));

                        //get staging property fields
                        col.Aggregate.Project = new ProjectPart(GetProjectPart().ToArray());

                        if (DoesQueryContainFunction)
                            col.Aggregate.GroupBy = new GroupByPart(GetGroupByPart().ToArray());
                    }

                    if (OrderByExpr != null)
                        col.Aggregate.OrderBy = GetOrderPart(Target);

                    if (RestrictExpr != null)
                        col.Aggregate.Restrict = new RestrictPart(RestrictExpr.Value);
                }
            }

            return collections;
        }

        #endregion

        #region Modify

        private CollectionPart[] ConvertToModifyModel()
        {
            //get collection(s)
            var collections = GetCollectionPart();

            if (collections.Count() > 0)
            {
                var parts = new List<IExpression>();

                foreach (var col in collections)
                {
                    //confitions
                    parts.Add(new ConditionPart(GetLogicalPart().ToArray()));
                    //set property fields
                    parts.AddRange(SetValueParts(false).ToArray());

                    //set property fields
                    col.Update = new UpdatePart(parts.ToArray());
                    col.Update.CollectionName = col.Target;
                }
            }

            return collections;
        }

        #endregion

        #region Add

        private CollectionPart[] ConvertToAddModel()
        {
            //get collection(s)
            var collections = GetCollectionPart();

            if (collections.Count() > 0)
            {
                foreach (var col in collections)
                {
                    col.Insert = new InsertPart(SetValueParts(true));
                    col.Insert.CollectionName = col.Target;
                }
            }

            return collections;
        }

        #endregion

        private CollectionPart[] GetCollectionPart()
        {
            var collections = new List<CollectionPart>();

            if (DataModelExpr != null)
            {
                foreach (var expr in DataModelExpr.Value)
                {
                    if (expr is DataExpr)
                    {
                        var data = (DataExpr)expr;

                        var model = Assistor.USchema.Single(x => x.View.Name == data.Value);

                        //get link references based on the data model
                        var references = model.View.Resources
                                        .Where(x => x.Property == "base")
                                        .SelectMany(x => x.Link.Where(x => x.Target == Enum.GetName(typeof(Database), Database.MONGODB).ToLower()))
                                        .First();

                        //get all linked properties, model, etc
                        var native = Assistor.NSchema[(int)Database.MONGODB]
                        .SelectMany(s => s.Model)
                              .Where(x => x.Name == references.Reference && x.Type == "collection")
                              .FirstOrDefault();

                        if (native != null)
                        {
                            collections.Add(new CollectionPart(native.Name, data.Value, data.AliasIdentifier));
                            break;
                        }
                    }
                }
            }

            return collections.ToArray();
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

                        var operatorPart = new OperatorPart(operatorExpr.Operator, Common.Helpers.Utils.Database.MONGODB);
                        var comparePart = new ComparePart(operatorExpr.Compare, Common.Helpers.Utils.Database.MONGODB);

                        var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);
                        var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                        if(leftPart != null && rightPart != null)
                            parts.Add(new LogicalPart(leftPart, operatorPart, rightPart, comparePart));
                    }
                }
            }

            return parts.ToArray();
        }

        private IExpression[] GetProjectPart()
        {
            var parts = new List<IExpression>();

            if (DeclareExpr != null)
            {
                //set _id column first
                foreach (var declare in DeclareExpr.Value)
                {
                    dynamic baseExpr = declare is PropertyExpr ? ((PropertyExpr)declare) :
                                           declare is JsonExpr ? ((JsonExpr)declare) : ((FunctionExpr)declare);

                    if (baseExpr is not FunctionExpr)
                    {
                        var property = GetMappedProperty(baseExpr, Target);

                        if (property != null && property.Link != null)
                        {
                            var field = new ProjectFieldPart(property);
                            
                            parts.Add(field);

                            if(!field.Ignore)
                                parts.Add(new SeparatorPart(","));
                        }
                    }
                    else
                    {
                        var expr = (FunctionExpr)baseExpr;

                        foreach (var functionExpr in expr.Value)
                        {
                            dynamic nestedExpr = functionExpr is PropertyExpr ? ((PropertyExpr)functionExpr) : ((JsonExpr)functionExpr);

                            var property = GetMappedProperty(nestedExpr, Target);

                            if (property != null && property.Link != null)
                            {
                                var field = new ProjectFieldPart(property, true);

                                parts.Add(field);

                                if (!field.Ignore)
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

        private IExpression[] GetPropertyPart()
        {
            var parts = new List<IExpression>();

            if (DeclareExpr != null)
            {
                foreach (var part in DeclareExpr.Value)
                {
                    if (part is PropertyExpr)
                    {
                        var propertyExpr = (PropertyExpr)part;

                        var mappedProperty = GetMappedProperty(propertyExpr, Target);

                        if (mappedProperty != null && mappedProperty.Link != null)
                        {
                            var field = new PropertyPart(mappedProperty);

                            parts.Add(field);

                            if (!field.Ignore)
                                parts.Add(new SeparatorPart(","));
                        }
                    }
                    else if (part is FunctionExpr)
                    {
                        var functionExpr = (FunctionExpr)part;

                        foreach (var func in functionExpr.Value)
                        {
                            dynamic @base = func is PropertyExpr ? ((PropertyExpr)func) : ((JsonExpr)func);

                            var mappedProperty = GetMappedProperty(@base, Target);

                            if (mappedProperty != null && mappedProperty.Link != null)
                            {
                                var field = new PropertyPart(mappedProperty);

                                parts.Add(field);

                                if (!field.Ignore)
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

        private IExpression[] GetGroupByPart()
        {
            var parts = new List<IExpression>();

            if (DeclareExpr != null)
            {
                //set _id column first
                foreach (var declare in DeclareExpr.Value)
                {
                    dynamic baseExpr = declare is PropertyExpr ? ((PropertyExpr)declare) :
                                           declare is JsonExpr ? ((JsonExpr)declare) : ((FunctionExpr)declare);

                    if (baseExpr is not FunctionExpr)
                    {
                        var property = GetMappedProperty(baseExpr, Target);

                        if (property != null && property?.Link != null)
                        {
                            var field = new GroupByFieldPart(property);

                            parts.Add(field);

                            if (!field.Ignore)
                                parts.Add(new SeparatorPart(","));
                        }
                    }
                    else
                    {
                        var expr = (FunctionExpr)baseExpr;

                        foreach (var functionExpr in expr.Value)
                        {
                            dynamic nestedExpr = functionExpr is PropertyExpr ? ((PropertyExpr)functionExpr) : ((JsonExpr)functionExpr);

                            var property = GetMappedProperty(nestedExpr, Target);

                            if (property != null && property?.Link != null)
                            {
                                var field = new FunctionFieldPart(property);

                                parts.Add(
                                    new NativeFunctionPart(
                                        field, expr.Type)
                                    );

                                if (!field.Ignore)
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

        private IExpression[] SetValueParts(bool insert)
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

                            var operatorPart = new OperatorPart(operatorExpr.Operator, Common.Helpers.Utils.Database.MONGODB);

                            var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);
                            var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                            if (leftPart != null && rightPart != null)
                            {
                                exprs.Add(new SetValuePart(leftPart, operatorPart, rightPart));
                                exprs.Add(new SeparatorPart(","));
                            }
                        }
                    }

                    if (exprs.Count > 0)
                    {
                        exprs.RemoveAt(exprs.Count - 1);

                        if (insert)
                            parts.Add(new AddPart(exprs.ToArray()));
                        else
                            parts.Add(new SetPart(exprs.ToArray()));

                        parts.Add(new SeparatorPart(","));
                    }
                }
            }

            if (parts.Count > 0)
                parts.RemoveAt(parts.Count - 1);

            return parts.ToArray();
        }

        private IExpression[] GetUnwindPart(string database)
        {
            var parts = new List<IExpression>();

            if (DeclareExpr != null)
            {
                foreach (var expr in DeclareExpr.Value)
                {
                    var json = GetUnwindArrayPart(expr, database);

                    if (json != null)
                    {
                        var jsonParts = parts.Where(x => x.GetType().Equals(typeof(UnwindArrayPart))).Cast<UnwindArrayPart>().ToList();

                        if (!jsonParts.Exists(x => x.Name == json.Name))
                        {
                            parts.Add(json);
                            parts.Add(new SeparatorPart(","));
                        }

                    }
                }

                if (parts.Count > 0)
                    parts.RemoveAt(parts.Count - 1);
            }

            return parts.ToArray();
        }

        private UnwindArrayPart? GetUnwindArrayPart(BaseExpr baseExpr, string database)
        {
            if (baseExpr is FunctionExpr)
            {
                foreach (var expr in ((FunctionExpr)baseExpr).Value)
                    return GetUnwindArrayPart(expr, database);
            }
            else
            {
                var mappedProperty = GetMappedProperty(baseExpr, database);

                if (mappedProperty != null && mappedProperty.Link != null)
                {
                    if (!string.IsNullOrEmpty(mappedProperty.Link.Property))
                    {
                        var parts = mappedProperty.Link.Property.Split(".");

                        if (parts.Length > 0)
                        {
                            var parent = Assistor.NSchema[(int)Database.MONGODB].SelectMany(x => x.Model.Where(x => x.Name == parts[0])).FirstOrDefault();

                            if (parent != null)
                            {
                                if (parent.Type == "array")
                                    return new UnwindArrayPart(mappedProperty);
                                else
                                {
                                    for (int i = 1; i <= parts.Length - 1; i++)
                                    {
                                        var child = parent.Properties.SingleOrDefault(x => x.Property == parts[i]);

                                        if (child != null)
                                        {
                                            if (child.Type == "array")
                                                return new UnwindArrayPart(mappedProperty);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
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

        private Codex BuildCodex(CollectionPart[] parts)
        {
            FromProperty from;
            ToProperty to;

            var codex = new Codex
            {
                Target = Database.MONGODB,
                PropertyModel = new List<Model>(),
                DataModel = DataModelExpr
            };

            foreach (var collection in parts)
            {
                var model = new Model
                {
                    Name = collection.Source,
                    Views = new Dictionary<FromProperty, ToProperty>()
                };

                if (collection.Find != null)
                {
                    if (collection.Find.Field != null)
                    {
                        foreach (var field in collection.Find.Field.Fields)
                        {
                            if (field is PropertyPart)
                            {
                                var property = (PropertyPart)field;

                                from = new FromProperty
                                {
                                    Name = property.Name,
                                    Alias = property.AliasName
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
                else if (collection.Aggregate != null)
                {
                    if (collection.Aggregate.GroupBy != null)
                    {
                        foreach (var field in collection.Aggregate.GroupBy.Fields)
                        {
                            if (field is GroupByFieldPart)
                            {
                                var property = (GroupByFieldPart)field;

                                from = new FromProperty
                                {
                                    Name = property.Name,
                                    Alias = property.Alias,
                                };

                                to = new ToProperty
                                {
                                    Name = property.Source,
                                };

                                model.Views.Add(from, to);
                            }
                            else if (field is FunctionFieldPart)
                            {
                                var property = (FunctionFieldPart)field;

                                from = new FromProperty
                                {
                                    Name = property.Name,
                                    Alias = property.Alias,
                                };

                                to = new ToProperty
                                {
                                    Name = property.Source,
                                };

                                model.Views.Add(from, to);
                            }
                        }
                    }
                    else if (collection.Aggregate.Project != null)
                    {
                        foreach (var field in collection.Aggregate.Project.Fields)
                        {
                            if (field is ProjectFieldPart)
                            {
                                var property = (ProjectFieldPart)field;

                                from = new FromProperty
                                {
                                    Name = property.Name,
                                    Alias = property.Alias,
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

            return codex;
        }

        #endregion
    }
}