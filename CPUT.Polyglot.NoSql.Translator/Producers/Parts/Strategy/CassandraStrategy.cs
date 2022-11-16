using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Parser;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Cassandra;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using BaseExpr = CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using Component = CPUT.Polyglot.NoSql.Parser.Syntax.Component;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class CassandraStrategy : StrategyPart
    {
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

        public override string Fetch(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting Cassandra - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToSelectModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new CassandraGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Modify(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting Cassandra - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToUpdateModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new CassandraGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Add(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting Cassandra - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new CassandraGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        private List<IExpression> ConvertToSelectModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> queryParts = new List<IExpression>();
            List<IExpression> logicalParts = new List<IExpression>();
            List<IExpression> propertyParts = new List<IExpression>();

            //set expression parts
            var DeclareExpr = (Component.DeclareExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DeclareExpr)));
            var DataModelExpr = (Component.DataModelExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DataModelExpr)));
            var LinkExpr = (Component.LinkExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.LinkExpr)));
            var FilterExpr = (Component.FilterExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.FilterExpr)));
            var GroupByExpr = (Component.GroupByExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.GroupByExpr)));
            var RestrictExpr = (Component.RestrictExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.RestrictExpr)));
            var OrderByExpr = (Component.OrderByExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.OrderByExpr)));

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DataModelExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();

            //select
            propertyParts = GetPropertyPart(DeclareExpr, mapperLinks, schemas).ToList();

            if (propertyParts.Count > 0)
                propertyParts.RemoveAt(propertyParts.Count - 1);

            targetModel.Add(new SelectPart(propertyParts.ToArray()));

            if (targetModel.Count > 0)
            {
                //from 
                targetModel.Add(new FromPart(GetTablePart(DataModelExpr, mapperLinks)));

                if (FilterExpr != null)
                    targetModel.Add(new ConditionPart(GetLogicalPart(FilterExpr, mapperLinks, schemas)));

                if (OrderByExpr != null)
                {
                    var mappedProperty = GetMappedProperty(mapperLinks, OrderByExpr.Value, "cassandra");

                    if (mappedProperty != null)
                        targetModel.Add(new OrderByPart( mappedProperty.Property, "", new DirectionPart(OrderByExpr.Direction)));
                }

                if (RestrictExpr != null)
                    targetModel.Add(new RestrictPart(RestrictExpr.Value));
            }

            return targetModel;
        }

        private List<IExpression> ConvertToUpdateModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> queryParts = new List<IExpression>();
            List<IExpression> logicalParts = new List<IExpression>();
            List<IExpression> propertyParts = new List<IExpression>();

            //set expression parts
            var DeclareExpr = (Component.DeclareExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DeclareExpr)));
            var PropertiesExpr = (Component.PropertiesExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.PropertiesExpr)));
            var FilterExpr = (Component.FilterExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.FilterExpr)));

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DeclareExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();


            //update 
            targetModel.Add(new UpdatePart(GetTablePart(DeclareExpr, mapperLinks)));

            if (targetModel.Count > 0)
            {
                //set values
                propertyParts = GetSetValuePart(PropertiesExpr, mapperLinks, schemas).ToList();

                if (propertyParts.Count > 0)
                    propertyParts.RemoveAt(propertyParts.Count - 1);

                targetModel.Add(new SetPart(propertyParts.ToArray()));


                if (FilterExpr != null)
                    targetModel.Add(new ConditionPart(GetLogicalPart(FilterExpr, mapperLinks, schemas)));
            }

            return targetModel;
        }

        private List<IExpression> ConvertToAddModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> queryParts = new List<IExpression>();
            List<IExpression> logicalParts = new List<IExpression>();
            List<IExpression> propertyParts = new List<IExpression>();

            //set expression parts
            var DeclareExpr = (Component.DeclareExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DeclareExpr)));
            var PropertiesExpr = (Component.PropertiesExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.PropertiesExpr)));

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DeclareExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();

            //insert 
            targetModel.Add(new InsertPart(GetTablePart(DeclareExpr, mapperLinks)));

            if (targetModel.Count > 0)
            {
                //set values
                propertyParts = GetInsertValuePart(PropertiesExpr, mapperLinks, schemas).ToList();

                targetModel.Add(new ValuesPart(propertyParts.ToArray()));
            }

            return targetModel;
        }

        private IExpression[] GetPropertyPart(Component.DeclareExpr declareExpr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<IExpression> expressions = new List<IExpression>();
            PropertyPart propertyPart = null;

            DataModelExpr dataModelExpr = null;
            PropertyExpr propertyExpr;
            FunctionExpr functionExpr;

            Properties properties;
            Link mappedProperty;


            foreach (var part in declareExpr.Value)
            {
                if (part is PropertyExpr)
                {
                    propertyExpr = (PropertyExpr)part;

                    mappedProperty = GetMappedProperty(mapperLinks, propertyExpr.Value, "cassandra");

                    if (mappedProperty != null)
                    {
                        properties = schemas
                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                            .Where(x => x.Property == mappedProperty.Property)
                            .First();

                        expressions.Add(new PropertyPart(properties, mappedProperty));
                        expressions.Add(new SeparatorPart(","));
                    };
                }
                else if (part is FunctionExpr)
                {
                    functionExpr = (FunctionExpr)part;

                    foreach (var func in functionExpr.Value)
                    {
                        propertyExpr = (PropertyExpr)func;

                        mappedProperty = GetMappedProperty(mapperLinks, propertyExpr.Value, "cassandra");

                        if (mappedProperty != null)
                        {
                            properties = schemas
                                        .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                        .Where(x => x.Property == mappedProperty.Property)
                                        .First();

                            expressions.Add(new PropertyPart(properties, mappedProperty));
                            expressions.Add(new SeparatorPart(","));
                        }
                    }
                }
            }

            return expressions.ToArray();
        }

        private IExpression[] GetSetValuePart(PropertiesExpr propertiesExpr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<IExpression> setValueParts = new List<IExpression>();
            PropertyPart leftPart = null;
            PropertyPart rightPart = null;
            OperatorPart operatorPart = null;
            
            GroupExpr group = null;
            OperatorExpr @operator;
            TermExpr left;
            Link leftMap;
            Link rightMap;
            Properties properties;

            foreach (var part in propertiesExpr.Value)
            {
                if (part is GroupExpr)
                {
                    group = (GroupExpr)part;
                    @operator = (OperatorExpr)group.Value;

                    left = (TermExpr)@operator.Left;

                    operatorPart = new OperatorPart(@operator.Operator, Common.Helpers.Utils.Database.CASSANDRA);

                    leftMap = GetMappedProperty(mapperLinks, left.Value, "cassandra");

                    if (leftMap != null)
                    {
                        properties = schemas
                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                            .First(x => x.Property == leftMap.Property);

                        leftPart = new PropertyPart(properties, leftMap);
                    }

                    if (@operator.Right is TermExpr)
                    {
                        var rightTerm = (TermExpr)@operator.Right;

                        rightMap = GetMappedProperty(mapperLinks, rightTerm.Value, "cassandra");

                        if (rightMap != null)
                        {
                            properties = schemas
                                .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                .First(x => x.Property == rightMap.Property);

                            rightPart = new PropertyPart(properties, rightMap);
                        }
                    }
                    else if (@operator.Right is StringLiteralExpr)
                        rightPart = new PropertyPart((StringLiteralExpr)@operator.Right);
                    else if (@operator.Right is NumberLiteralExpr)
                        rightPart = new PropertyPart((NumberLiteralExpr)@operator.Right);

                    if (leftPart != null && rightPart != null)
                    {
                        setValueParts.Add(new SetValuePart(leftPart, operatorPart, rightPart));

                        if (propertiesExpr != null)
                            setValueParts.Add(new SeparatorPart(","));
                    }
                }
            }

            return setValueParts.ToArray();
        }

        private IExpression[] GetInsertValuePart(PropertiesExpr propertiesExpr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<IExpression> insertValueParts = new List<IExpression>();
            List<IExpression> leftProperties = new List<IExpression>();
            List<IExpression> rightProperties = new List<IExpression>();
            PropertyPart leftPart = null;
            PropertyPart rightPart = null;

            GroupExpr group = null;
            OperatorExpr @operator;
            TermExpr left;
            Link leftMap;
            Link rightMap;
            Properties properties;

            foreach (var part in propertiesExpr.Value)
            {
                if (part is GroupExpr)
                {
                    group = (GroupExpr)part;
                    @operator = (OperatorExpr)group.Value;

                    left = (TermExpr)@operator.Left;

                    leftMap = GetMappedProperty(mapperLinks, left.Value, "cassandra");

                    if (leftMap != null)
                    {
                        properties = schemas
                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                            .First(x => x.Property == leftMap.Property);

                        leftPart = new PropertyPart(properties, leftMap);
                    }

                    if (@operator.Right is TermExpr)
                    {
                        var rightTerm = (TermExpr)@operator.Right;

                        rightMap = GetMappedProperty(mapperLinks, rightTerm.Value, "cassandra");

                        if (rightMap != null)
                        {
                            properties = schemas
                                .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                .First(x => x.Property == rightMap.Property);

                            rightPart = new PropertyPart(properties, rightMap);
                        }
                    }
                    else if (@operator.Right is StringLiteralExpr)
                        rightPart = new PropertyPart(((StringLiteralExpr)@operator.Right));
                    else if (@operator.Right is NumberLiteralExpr)
                        rightPart = new PropertyPart(((NumberLiteralExpr)@operator.Right));

                    if (leftPart != null && rightPart != null)
                    {
                        leftProperties.Add(leftPart);
                        leftProperties.Add(new SeparatorPart(","));

                        rightProperties.Add(rightPart);
                        rightProperties.Add(new SeparatorPart(","));
                    }
                }
            }

            if (leftProperties.Count > 0)
                leftProperties.RemoveAt(leftProperties.Count - 1);

            if (rightProperties.Count > 0)
                rightProperties.RemoveAt(rightProperties.Count - 1);

            insertValueParts.Add(new InsertValuePart(leftProperties.ToArray(), rightProperties.ToArray()));

            return insertValueParts.ToArray();
        }

        private IExpression[] GetTablePart(BaseExpr.BaseExpr expr, List<MappedSource> mapperLinks)
        {
            List<IExpression> fromParts = new List<IExpression>();

            DeclareExpr declareExpr = null;
            DataModelExpr dataModelExpr = null;

            if (expr is DeclareExpr)
                declareExpr = (DeclareExpr)expr;
            else if (expr is DataModelExpr)
                dataModelExpr = (DataModelExpr)expr;

            foreach (var part in (declareExpr != null ? declareExpr.Value : dataModelExpr.Value))
            {
                if (part is DataExpr)
                {
                    DataExpr data = (DataExpr)part;

                    var map = mapperLinks
                        .Where(x => x.Name == data.Value)
                        .SelectMany(x => x.Resources
                                            .SelectMany(x => x.Link))
                        .Where(t => t.Target == "cassandra")
                        .FirstOrDefault();

                    if (map != null)
                        fromParts.Add(new TablePart(map.Reference, ""));
                }
            }

            return fromParts.ToArray();
        }

        private LogicalPart[] GetLogicalPart(Component.FilterExpr filterExpr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<LogicalPart> logicalParts = new List<LogicalPart>();
            PropertyPart leftPart = null;
            PropertyPart rightPart = null;
            OperatorPart operatorPart = null;
            ComparePart comparePart = null;

            PropertiesExpr propertiesExpr = null;
            GroupExpr group = null;
            OperatorExpr @operator;
            TermExpr left;
            Link leftMap;
            Link rightMap;
            Properties properties;

            foreach (var part in filterExpr.Value)
            {
                if (part is GroupExpr)
                {
                    group = (GroupExpr)part;
                    @operator = (OperatorExpr)group.Value;

                    left = (TermExpr)@operator.Left;

                    operatorPart = new OperatorPart(@operator.Operator, Common.Helpers.Utils.Database.CASSANDRA);
                    comparePart = new ComparePart(@operator.Compare, Common.Helpers.Utils.Database.CASSANDRA);

                    leftMap = GetMappedProperty(mapperLinks, left.Value, "cassandra");

                    if (leftMap != null)
                    {
                        properties = schemas
                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                            .First(x => x.Property == leftMap.Property);

                        leftPart = new PropertyPart(properties, leftMap);
                    }

                    if (@operator.Right is TermExpr)
                    {
                        var rightTerm = (TermExpr)@operator.Right;

                        rightMap = GetMappedProperty(mapperLinks, rightTerm.Value, "cassandra");

                        if (rightMap != null)
                        {
                            properties = schemas
                                .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                .First(x => x.Property == rightMap.Property);

                            rightPart = new PropertyPart(properties, rightMap);
                        }
                    }
                    else if (@operator.Right is StringLiteralExpr)
                        rightPart = new PropertyPart((StringLiteralExpr)@operator.Right);
                    else if (@operator.Right is NumberLiteralExpr)
                        rightPart = new PropertyPart((NumberLiteralExpr)@operator.Right);

                    if (leftPart != null && rightPart != null)
                        logicalParts.Add(new LogicalPart(leftPart, operatorPart, rightPart, comparePart));
                }
            }

            return logicalParts.ToArray();
        }
    }
}
