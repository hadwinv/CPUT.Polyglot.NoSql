using CPUT.Polyglot.NoSql.Models.Views.Shared;
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
using System.Linq;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

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

        public override string Fetch()
        {
            Console.WriteLine("Starting MongoDbPart - Fetch");

            CollectionPart[] targetQuery;
            
            //set expression parts
            targetQuery = FindOrAggregateModel();

            //pass query expresion
            var match = new QueryPart(targetQuery);

            //initialise mongodb query generator
            var query = new StringBuilder();
            var generator = new MongoDbGenerator(query, DoAggregateModel ? MongoDBFetchType.Aggregate : MongoDBFetchType.Find);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Modify()
        {
            Console.WriteLine("Starting MongoDbPart - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToModifyModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise mongodb query generator
            var generator = new MongoDbGenerator(query, MongoDBFetchType.None);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Add()
        {
            Console.WriteLine("Starting MongoDbPart - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise mongodb query generator
            var generator = new MongoDbGenerator(query, MongoDBFetchType.None);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
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
                        //unwind complex field i.e. json
                        col.Aggregate.Unwind = new UnwindGroupPart(GetUnwindPart(Target).ToArray());

                        //get staging property fields
                        col.Aggregate.Project = new ProjectPart(GetProjectPart().ToArray());

                        if(DoesQueryContainFunction)
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
                }
            }

            return collections;
        }

        #endregion

        private CollectionPart[] GetCollectionPart()
        {
            var collections = new List<CollectionPart>();

            if(DataModelExpr != null)
            {
                foreach (var expr in DataModelExpr.Value)
                {
                    if (expr is DataExpr)
                    {
                        var data = (DataExpr)expr;

                        var model = Assistor.USchema.Single(x => x.View.Name == data.Value);

                        //get link references based on the data model
                        var references = model.View.Resources
                                        .SelectMany(x => x.Link.Where(x => x.Target == Enum.GetName(typeof(Database), Database.MONGODB).ToLower()))
                                        .Select(x => x.Reference)
                                        //.Distinct()
                                        .ToList();

                        if(references != null)
                        {
                            //get all linked properties, model, etc
                            var native = Assistor.NSchema[(int)Database.MONGODB]
                            .SelectMany(s => s.Model)
                                  .Where(x => references.Contains(x.Name) && x.Type == "collection")
                                  .FirstOrDefault();

                            if (native != null)
                            {
                                collections.Add(new CollectionPart(native.Name, data.Value, data.AliasIdentifier));
                                break;
                            }
                        }
                    }
                }
            }

            return collections.ToArray();
        }

        private LogicalPart[] GetLogicalPart()
        {
            var parts = new List<LogicalPart>();
            
            if(FilterExpr != null)
            {
                foreach (var part in FilterExpr.Value)
                {
                    if (part is GroupExpr)
                    {
                        var groupExpr = (GroupExpr)part;
                        var operatorExpr = (OperatorExpr)groupExpr.Value;

                        var operatorPart = new OperatorPart(operatorExpr.Operator, Common.Helpers.Utils.Database.MONGODB);
                        var comparePart = new ComparePart(operatorExpr.Compare, Common.Helpers.Utils.Database.MONGODB);

                        var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target, (int)Database.MONGODB);
                        var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target, (int)Database.MONGODB);

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
                            parts.Add(new ProjectFieldPart(property));
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
                                parts.Add(new ProjectFieldPart(property, true));
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

            if(DeclareExpr != null)
            {
                foreach (var part in DeclareExpr.Value)
                {
                    if (part is PropertyExpr)
                    {
                        var propertyExpr = (PropertyExpr)part;

                        var mappedProperty = GetMappedProperty(propertyExpr, Target);

                        if (mappedProperty != null && mappedProperty.Link != null)
                        {
                            parts.Add(new PropertyPart(mappedProperty));
                            parts.Add(new SeparatorPart(","));
                        };
                    }
                    else if (part is FunctionExpr)
                    {
                        var functionExpr = (FunctionExpr)part;

                        foreach (var func in functionExpr.Value)
                        {
                            BaseExpr @base;

                            if (func is PropertyExpr)
                                @base = (PropertyExpr)func;
                            else
                                @base = (JsonExpr)func;

                            var mappedProperty = GetMappedProperty(@base, Target);

                            if (mappedProperty != null && mappedProperty.Link != null)
                            {
                                parts.Add(new PropertyPart(mappedProperty));
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

                        if (property != null && property.Link != null)
                        {
                            parts.Add(new GroupByFieldPart(property));
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
                                parts.Add(
                                    new NativeFunctionPart(
                                        new FunctionFieldPart(property), expr.Type)
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

                            var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target, (int)Database.MONGODB);
                            var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target, (int)Database.MONGODB);

                            exprs.Add(new SetValuePart(leftPart, operatorPart, rightPart));
                            exprs.Add(new SeparatorPart(","));
                        }
                    }

                    if (exprs.Count > 0)
                        exprs.RemoveAt(exprs.Count - 1);

                    if (insert)
                        parts.Add(new AddPart(exprs.ToArray()));
                    else
                        parts.Add(new SetPart(exprs.ToArray()));

                    parts.Add(new SeparatorPart(","));
                }
            }

            if (parts.Count > 0)
                parts.RemoveAt(parts.Count - 1);

            return parts.ToArray();
        }

        private UnwindJsonPart[] GetUnwindPart(string database)
        {
            var parts = new List<UnwindJsonPart>();

            if (DeclareExpr != null)
            {
                foreach (var expr in DeclareExpr.Value)
                {
                    var json = GetJsonPart(expr, database);

                    if (json != null)
                    {
                        if (!parts.Exists(x => x.Name == json.Name))
                            parts.Add(json);
                    }
                }
            }

            return parts.ToArray();
        }

        private UnwindJsonPart? GetJsonPart(BaseExpr baseExpr, string database)
        {
            if (baseExpr is JsonExpr)
            {
                var mappedProperty = GetMappedProperty(baseExpr, database);

                

                if (mappedProperty != null && mappedProperty.Link != null)
                {
                    if(mappedProperty.Link.Property.IndexOf(".") > -1)
                        return new UnwindJsonPart(mappedProperty);

                }

            }
            else if (baseExpr is FunctionExpr)
            {
                foreach (var expr in ((FunctionExpr)baseExpr).Value)
                    return GetJsonPart(expr, database);
            }

            return default;
        }

        #endregion
    }
}