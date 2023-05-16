using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
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
using CPUT.Polyglot.NoSql.Models.Translator;
using Cassandra.Mapping;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using System.Xml.Linq;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class Neo4jStrategy : StrategyPart
    {
        protected string Target = "neo4j";

        private IExpression[] _unwindParts { get; set; }

        public IExpression[] UnwindParts { 
            get
            {
                if (_unwindParts == null)
                    _unwindParts = GetUnwindPart();

                return _unwindParts;
            }
        }


        public override string Alter()
        {
            throw new NotImplementedException();
        }

        public override string Create()
        {
            throw new NotImplementedException();
        }

        public override string Describe()
        {
            throw new NotImplementedException();
        }

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
                if (this.FilterExpr != null)
                    parts.Add(new ConditionPart(GetLogicalPart().ToArray()));

                //get unwind fields
                parts.AddRange(UnwindParts);

                //get property fields
                parts.Add(new ReturnPart(GetPropertyPart().ToArray()));

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

            // var innerParts = new List<IExpression>();

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

                            //apply correct alias
                            ApplyUnwindedAlias(mappedProperty, ref propertyExpr);

                            parts.Add(new PropertyPart(mappedProperty, propertyExpr));
                            parts.Add(new SeparatorPart(","));
                        }
                    }
                    else if (part is FunctionExpr)
                    {
                        var expr = (FunctionExpr)part;

                        foreach (var func in expr.Value)
                        {
                            var @base = (PropertyExpr)func;

                            //if (func is PropertyExpr)
                            //    @base = (PropertyExpr)func;
                            //else
                            //    @base = (JsonExpr)func;

                            var mappedProperty = GetMappedProperty(@base, Target);

                            if (mappedProperty != null)
                            {
                                var properties = Assistor.NSchema
                                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                            .Where(x => x.Property == mappedProperty.Property)
                                            .First();

                                //apply correct alias
                                ApplyUnwindedAlias(mappedProperty, ref @base);

                                parts.Add(new NativeFunctionPart(
                                        new PropertyPart( mappedProperty, @base), expr.Type)
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
                                    innerParts.Add(new UnwindJsonPart(propertyExpr, parentModel, model.Name));
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
                                        innerParts.Add(new UnwindJsonPart(propertyExpr, parentModel, model.Name));
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
                    parts.Add(new UnwindPart(innerParts.ToArray()));
                }
            }

            return parts.ToArray();
        }

        private IExpression[] GetInsertValuePart(bool insert)
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

                            if (insert)
                                exprs.Add(new InsertNodePart(leftPart, operatorPart, rightPart));
                            else
                                exprs.Add(new SetValuePart(leftPart, operatorPart, rightPart));

                            exprs.Add(new SeparatorPart(","));
                        }
                    }

                    if (exprs.Count > 0)
                        exprs.RemoveAt(exprs.Count - 1);

                    if (insert)
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

        private void ApplyUnwindedAlias(Link mappedProperty, ref PropertyExpr propertyExpr)
        {
            if (UnwindParts != null && UnwindParts.Count() > 0)
            {
                var unwind = (UnwindPart?)UnwindParts.SingleOrDefault(x => x.GetType().Equals(typeof(UnwindPart)));

                if (unwind != null)
                {
                    foreach (var field in unwind.Fields.Where(x => x.GetType().Equals(typeof(UnwindJsonPart))))
                    {
                        var unwindJsonPart = (UnwindJsonPart)field;

                        if (unwindJsonPart.Name == mappedProperty.Reference)
                        {
                            propertyExpr.AliasIdentifier = unwindJsonPart.UnwindAliasIdentifier;
                            break;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
