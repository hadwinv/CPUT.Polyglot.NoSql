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
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using BaseExpr = CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using Pipelines.Sockets.Unofficial.Arenas;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class CassandraStrategy : StrategyPart
    {
        protected string Target = "cassandra";

        public override string Fetch()
        {
            Console.WriteLine("Starting Cassandra - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToSelectModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new CassandraGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Modify()
        {
            Console.WriteLine("Starting Cassandra - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToUpdateModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new CassandraGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Add()
        {
            Console.WriteLine("Starting Cassandra - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new CassandraGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
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

                if (this.OrderByExpr != null)
                    parts.Add(this.GetOrderPart(Target));

                if (RestrictExpr != null)
                    parts.Add(new RestrictPart(RestrictExpr.Value));
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

                            parts.Add(new PropertyPart(mappedProperty, propertyExpr));
                            parts.Add(new SeparatorPart(","));
                        };
                    }
                    else if (part is FunctionExpr)
                    {
                        var expr = (FunctionExpr)part;

                        foreach (var func in expr.Value)
                        {
                            BaseExpr.BaseExpr @base;

                            if (func is PropertyExpr)
                                @base = (PropertyExpr)func;
                            else
                                @base = (JsonExpr)func;

                            var mappedProperty = GetMappedProperty(@base, Target);

                            if (mappedProperty != null)
                            {
                                var properties = Assistor.NSchema
                                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                            .Where(x => x.Property == mappedProperty.Property)
                                            .First();

                                parts.Add(
                                    new NativeFunctionPart(
                                        new PropertyPart(mappedProperty, @base), expr.Type)
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
                            parts.Add(new TablePart(link.Reference, ""));
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
                    var exprs = new List<IExpression>();

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
    }
}
