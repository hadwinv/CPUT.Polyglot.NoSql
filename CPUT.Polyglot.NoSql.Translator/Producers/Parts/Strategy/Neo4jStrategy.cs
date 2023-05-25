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

            ////apply correct aliases
            //FindAndApplyUnwindedAlias(ref parts);

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
                        {
                            var relations = Assistor.NSchema
                                .SelectMany(x => x.Model
                                        .Where(x => x.Name == link.Reference && x.Relations != null)
                                .SelectMany(x => x.Relations)).ToList();

                            parts.Add(new NodePart(link.Reference, data.AliasIdentifier, relations.ToArray()));
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

                        var operatorPart = new OperatorPart(operatorExpr.Operator, Common.Helpers.Utils.Database.NEOJ4);
                        var comparePart = new ComparePart(operatorExpr.Compare, Common.Helpers.Utils.Database.NEOJ4);

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
                    if (part is PropertyExpr)
                    {
                        var propertyExpr = (PropertyExpr)part;

                        var mappedProperty = GetMappedProperty(propertyExpr, Target);

                        if (mappedProperty != null)
                        {
                            var properties = Assistor.NSchema
                                .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                .Where(x => x.Property == mappedProperty.Property)
                                .First();

                            ////apply correct alias
                            //ApplyUnwindedAlias(mappedProperty, ref propertyExpr);

                            parts.Add(new PropertyPart(mappedProperty, propertyExpr));
                            parts.Add(new SeparatorPart(","));
                        }
                    }
                    else if (part is FunctionExpr)
                    {
                        var expr = (FunctionExpr)part;

                        foreach (var func in expr.Value)
                        {
                            var propertyExpr = (PropertyExpr)func;

                            var mappedProperty = GetMappedProperty(propertyExpr, Target);

                            if (mappedProperty != null)
                            {
                                var properties = Assistor.NSchema
                                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                            .Where(x => x.Property == mappedProperty.Property)
                                            .First();

                                ////apply correct alias
                                //ApplyUnwindedAlias(mappedProperty, ref propertyExpr);

                                parts.Add(new NativeFunctionPart(
                                        new PropertyPart(mappedProperty, propertyExpr), expr.Type)
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

        private IExpression[] GetUnwindPart()
        {
            var parts = new List<IExpression>();

            if (DeclareExpr != null)
            {
                var innerParts = new List<IExpression>();
                var models = new List<Model>();

                foreach (var part in DeclareExpr.Value)
                {
                    if (part is PropertyExpr)
                    {
                        var propertyExpr = (PropertyExpr)part;

                        var mappedProperty = GetMappedProperty(propertyExpr, Target);

                        if (mappedProperty != null)
                        {
                            var model = Assistor.NSchema
                                .SelectMany(x => x.Model)
                                .Where(x => x.Name == mappedProperty.Reference)
                                .First();

                            if (model.Type == "json")
                            {
                                var parentModel = Assistor.NSchema
                                    .SelectMany(x => x.Model)
                                    .Where(x => x.Properties.Exists(x => x.Property == model.Name))
                                    .First();

                                if (!models.Exists(x => x.Name == parentModel.Name))
                                {
                                    innerParts.Add(new UnwindGraphPart(propertyExpr, parentModel, model));
                                    innerParts.Add(new SeparatorPart(","));

                                    models.Add(parentModel);
                                }
                            }
                        }
                    }
                    else if (part is FunctionExpr)
                    {
                        var functionExpr = (FunctionExpr)part;

                        foreach (var func in functionExpr.Value)
                        {
                            var propertyExpr = (PropertyExpr)func;

                            var mappedProperty = GetMappedProperty(propertyExpr, Target);

                            if (mappedProperty != null)
                            {
                                var model = Assistor.NSchema
                                 .SelectMany(x => x.Model)
                                 .Where(x => x.Name == mappedProperty.Reference)
                                 .First();

                                if (model.Type == "json")
                                {
                                    var parentModel = Assistor.NSchema
                                        .SelectMany(x => x.Model)
                                        .Where(x => x.Properties.Exists(x => x.Property == model.Name))
                                        .First();

                                    if (!models.Exists(x => x.Name == parentModel.Name))
                                    {
                                        innerParts.Add(new UnwindGraphPart(propertyExpr, parentModel, model));
                                        innerParts.Add(new SeparatorPart(","));

                                        models.Add(parentModel);
                                    }
                                }
                            }
                        }
                    }
                }

                if (innerParts.Count > 0)
                {
                    innerParts.RemoveAt(innerParts.Count - 1);
                    parts.Add(new UnwindGroupPart(innerParts.ToArray()));
                }
            }

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

                            var operatorPart = new OperatorPart(operatorExpr.Operator, Common.Helpers.Utils.Database.NEOJ4);

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
    }
}