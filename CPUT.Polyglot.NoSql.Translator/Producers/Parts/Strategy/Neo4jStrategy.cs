using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using MongoDB.Driver.Linq;
using System.Linq;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Models.Views;
using Pipelines.Sockets.Unofficial.Arenas;

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

        public override string Fetch()
        {
            Console.WriteLine("Starting Neo4jPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToSelectModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise neo4j query generator
            var generator = new Neo4jGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Modify()
        {
            Console.WriteLine("Starting Neo4jPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToUpdateModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new Neo4jGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Add()
        {
            Console.WriteLine("Starting Neo4jPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new Neo4jGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
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
                string[]? references = null;
                
                //get references
                if (DeclareExpr != null)
                    references = GetReferencesBasedOnDeclare(Target);
                else if (PropertiesExpr != null)
                    references = GetReferencesBasedOnProperties(Target);

                if (references != null)
                {
                    foreach (var reference in references)
                    {
                        foreach (var part in DataModelExpr.Value)
                        {
                            if (part is DataExpr)
                            {
                                var data = (DataExpr)part;

                                //get model
                                var model = Assistor.USchema.Single(x => x.View.Name == data.Value);
                                //get link references based on the data model
                                var link = model.View.Resources
                                                .SelectMany(x => x.Link.Where(x => x.Target == Enum.GetName(typeof(Database), Database.NEO4J).ToLower()))
                                                .FirstOrDefault(x => x.Reference == reference);

                                if(link != null)
                                {
                                    var native = Assistor.NSchema[(int)Database.NEO4J]
                                                .SelectMany(s => s.Model)
                                                .Where(x => x.Name == reference && x.Type == "node")
                                        .First();

                                    parts.Add(new NodePart(native.Name, data.AliasIdentifier, native.Relations?.ToArray()));
                                    break;
                                }
                            }
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

                        var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target,(int)Database.NEO4J);
                        var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target, (int)Database.NEO4J);

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

                            var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target, (int)Database.NEO4J);
                            var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target, (int)Database.NEO4J);

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
                    var json = GetJsonPart(expr, Target);

                    if (json != null)
                    {
                        if (graphs
                            .Where(x => x.GetType().Equals(typeof(UnwindGraphPart)))
                            .Count(x => ((UnwindGraphPart)x).UnwindProperty == json.UnwindProperty) < 1)
                        {
                            graphs.Add(json);
                            graphs.Add(new SeparatorPart(","));
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

        private UnwindGraphPart? GetJsonPart(BaseExpr baseExpr, string database)
        {
            if (baseExpr is JsonExpr)
            {
                var mappedProperty = GetMappedProperty(baseExpr, database);

                if(mappedProperty.Link != null)
                {
                    var reference = mappedProperty.Link.Property.Split(".")[0];

                    var model = Assistor.NSchema[(int)Database.NEO4J]
                                    .SelectMany(s => s.Model)
                                    .Where(x => x.Name == reference && x.Type == "json")
                                    .FirstOrDefault();

                    if(model != null)
                    {
                        mappedProperty.Link.Property = model.Name;

                        return new UnwindGraphPart(mappedProperty);
                    }
                    
                }
            }
            else if (baseExpr is FunctionExpr)
            {
                foreach (var expr in ((FunctionExpr)baseExpr).Value)
                    return GetJsonPart(expr, database);
            }

            return default;
        }
    }
}