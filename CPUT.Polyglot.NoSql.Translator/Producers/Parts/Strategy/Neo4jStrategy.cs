using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Parser;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using BaseExpr = CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using Component = CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Xml.Linq;
using Cassandra.Mapping;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class Neo4jStrategy : StrategyPart
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
            Console.WriteLine("Starting Neo4jPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToSelectModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise neo4j query generator
            var generator = new Neo4jGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Modify(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting Neo4jPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToUpdateModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new Neo4jGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Add(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting Neo4jPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise cassandra query generator
            var generator = new Neo4jGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public List<IExpression> ConvertToSelectModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> unwindJsonParts = new List<IExpression>();
            List<IExpression> propertyParts = new List<IExpression>();
            List<IExpression> logicalParts = new List<IExpression>();
            List<IExpression> nodeParts = new List<IExpression>();

            //set expression parts
            var DeclareExpr = (Component.DeclareExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DeclareExpr)));
            var DataModelExpr = (Component.DataModelExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DataModelExpr)));
            var LinkExpr = (Component.LinkExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.LinkExpr)));
            var FilterExpr = (Component.FilterExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.FilterExpr)));
            var RestrictExpr = (Component.RestrictExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.RestrictExpr)));
            var OrderByExpr = (Component.OrderByExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.OrderByExpr)));

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DataModelExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();

            //match
            targetModel.Add(new MatchPart(GetNodePart(DataModelExpr, mapperLinks, schemas)));

            if (targetModel.Count > 0)
            {
                if (FilterExpr != null)
                    logicalParts.AddRange(GetLogicalPart(FilterExpr, mapperLinks, schemas));

                if(logicalParts.Count > 0)
                    targetModel.Add(new ConditionPart(logicalParts.ToArray()));

                //get unwind fields
                unwindJsonParts.AddRange(GetUnwindJsonPart(DeclareExpr, mapperLinks));

                if (unwindJsonParts.Count > 0)
                {
                    unwindJsonParts.RemoveAt(unwindJsonParts.Count - 1);

                    //unwind collections
                    targetModel.Add(new UnwindPart(unwindJsonParts.ToArray()));
                }

                //get property fields
                propertyParts.AddRange(GetPropertyPart(DeclareExpr, mapperLinks, schemas));

                if (propertyParts.Count > 0)
                {
                    propertyParts.RemoveAt(propertyParts.Count - 1);

                    targetModel.Add(new ReturnPart(propertyParts.ToArray()));
                }

                if (OrderByExpr != null)
                {
                    var mappedProperty = GetMappedProperty(mapperLinks, OrderByExpr.Value, "neo4j");

                    if (mappedProperty != null)
                        targetModel.Add(new OrderByPart(mappedProperty.Property, mappedProperty.Reference.Substring(0, 3).ToLower(), new DirectionPart(OrderByExpr.Direction)));
                }

                if (RestrictExpr != null)
                    targetModel.Add(new RestrictPart(RestrictExpr.Value));
            }

            return targetModel;
        }

        public List<IExpression> ConvertToUpdateModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> propertyParts = new List<IExpression>();
            List<IExpression> logicalParts = new List<IExpression>();
            List<IExpression> nodeParts = new List<IExpression>();

            //set expression parts
            var DeclareExpr = (Component.DeclareExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DeclareExpr)));
            var PropertiesExpr = (Component.PropertiesExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.PropertiesExpr)));
            var FilterExpr = (Component.FilterExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.FilterExpr)));
            var RestrictExpr = (Component.RestrictExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.RestrictExpr)));
            var OrderByExpr = (Component.OrderByExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.OrderByExpr)));

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DeclareExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();

            //match
            targetModel.Add(new MatchPart(GetNodePart(DeclareExpr, mapperLinks, schemas)));

            if (targetModel.Count > 0)
            {
                if (FilterExpr != null)
                    logicalParts.AddRange(GetLogicalPart(FilterExpr, mapperLinks, schemas));

                if (logicalParts.Count > 0)
                    targetModel.Add(new ConditionPart(logicalParts.ToArray()));

                //set values
                propertyParts = GetSetValuePart(PropertiesExpr, mapperLinks, schemas).ToList();

                if (propertyParts.Count > 0)
                    propertyParts.RemoveAt(propertyParts.Count - 1);

                //if (OrderByExpr != null)
                //{
                //    var mappedProperty = GetMappedProperty(mapperLinks, OrderByExpr.Value, "cassandra");

                //    if (mappedProperty != null)
                //        targetModel.Add(new OrderByPart(mappedProperty.Property, new DirectionPart(OrderByExpr.Direction)));
                //}

                targetModel.Add(new SetPart(propertyParts.ToArray()));

                if (RestrictExpr != null)
                    targetModel.Add(new RestrictPart(RestrictExpr.Value));

               
            }

            return targetModel;
        }

        public List<IExpression> ConvertToAddModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> propertyParts = new List<IExpression>();
            List<IExpression> logicalParts = new List<IExpression>();
            List<IExpression> nodeParts = new List<IExpression>();

            //set expression parts
            var DeclareExpr = (Component.DeclareExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DeclareExpr)));
            var PropertiesExpr = (Component.PropertiesExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.PropertiesExpr)));
            var FilterExpr = (Component.FilterExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.FilterExpr)));

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DeclareExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();

            //match
            nodeParts.AddRange(GetNodePart(DeclareExpr, mapperLinks, schemas));

            if (nodeParts.Count > 0)
            {
                if (FilterExpr != null)
                    logicalParts.AddRange(GetLogicalPart(FilterExpr, mapperLinks, schemas));

                if (logicalParts.Count > 0)
                    nodeParts.Add(new ConditionPart(logicalParts.ToArray()));

                //set values
                propertyParts = GetInsertValuePart(PropertiesExpr, mapperLinks, schemas).ToList();

                if (propertyParts.Count > 0)
                    propertyParts.RemoveAt(propertyParts.Count - 1);


                nodeParts.Add(new ValuesPart(propertyParts.ToArray()));

                targetModel.Add(new InsertPart(nodeParts.ToArray()));
            }

            return targetModel;
        }

        private NodePart[] GetNodePart(BaseExpr.BaseExpr expr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<NodePart> nodeParts = new List<NodePart>();
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
                        .Where(t => t.Target == "neo4j")
                        .FirstOrDefault();

                    if (map != null)
                    {
                        var relations = schemas.SelectMany(x => x.Model.Where(x => x.Name == map.Reference && x.Relations != null).SelectMany(x => x.Relations)).ToList();

                        nodeParts.Add(new NodePart(map.Reference, map.Reference.Substring(0, 3), relations.ToArray()));
                    }
                }
            }

            return nodeParts.ToArray();
        }

        private IExpression[] GetUnwindJsonPart(BaseExpr.BaseExpr expr, List<MappedSource> mapperLinks)
        {
            List<IExpression> expressions = new List<IExpression>();

            DeclareExpr declareExpr = null;
            DataModelExpr dataModelExpr = null;
            PropertyExpr propertyExpr;
            FunctionExpr functionExpr;

            Link mappedProperty;


            if (expr is DeclareExpr)
                declareExpr = (DeclareExpr)expr;
            else if (expr is DataModelExpr)
                dataModelExpr = (DataModelExpr)expr;

            foreach (var part in (declareExpr != null ? declareExpr.Value : dataModelExpr.Value))
            {
                if (part is PropertyExpr)
                {
                    propertyExpr = (PropertyExpr)part;

                    mappedProperty = GetMappedProperty(mapperLinks, propertyExpr.Value, "neo4j");

                    if (mappedProperty != null)
                    {
                        var jsonParts = expressions.Where(x => x.GetType().Equals(typeof(UnwindJsonPart))).Select(x => (UnwindJsonPart)x).ToList();

                        if (!jsonParts.Exists(x => x.Name == mappedProperty.Reference_Property))
                        {
                            if (!string.IsNullOrEmpty(mappedProperty.Reference_Property))
                            {
                                expressions.Add(new UnwindJsonPart(mappedProperty));
                                expressions.Add(new SeparatorPart(","));
                            }
                        }
                    };
                }
                else if (part is FunctionExpr)
                {
                    functionExpr = (FunctionExpr)part;

                    foreach (var func in functionExpr.Value)
                    {
                        propertyExpr = (PropertyExpr)func;

                        mappedProperty = GetMappedProperty(mapperLinks, propertyExpr.Value, "neo4j");

                        if (mappedProperty != null)
                        {
                            var jsonParts = expressions.Where(x => x.GetType().Equals(typeof(UnwindJsonPart))).Select(x => (UnwindJsonPart)x).ToList();

                            if (!jsonParts.Exists(x => x.Name == mappedProperty.Reference_Property))
                            {
                                if (!string.IsNullOrEmpty(mappedProperty.Reference_Property))
                                {
                                    expressions.Add(new UnwindJsonPart(mappedProperty));
                                    expressions.Add(new SeparatorPart(","));
                                }
                            }
                        }
                    }
                }
            }

            return expressions.ToArray();
        }

        private LogicalPart[] GetLogicalPart(BaseExpr.BaseExpr expr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<LogicalPart> logicalParts = new List<LogicalPart>();
            PropertyPart leftPart = null;
            PropertyPart rightPart = null;
            OperatorPart operatorPart = null;
            ComparePart comparePart = null;

            FilterExpr filterExpr = null;
            PropertiesExpr propertiesExpr = null;
            GroupExpr group = null;
            OperatorExpr @operator;
            TermExpr left;
            Link leftMap;
            Link rightMap;
            Properties properties;

            if (expr is FilterExpr)
                filterExpr = (FilterExpr)expr;
            else if (expr is PropertiesExpr)
                propertiesExpr = (PropertiesExpr)expr;

            foreach (var part in (filterExpr != null ? filterExpr.Value : propertiesExpr.Value))
            {
                if (part is GroupExpr)
                {
                    group = (GroupExpr)part;
                    @operator = (OperatorExpr)group.Value;

                    left = (TermExpr)@operator.Left;

                    operatorPart = new OperatorPart(@operator.Operator, Common.Helpers.Utils.Database.NEOJ4);
                    comparePart = new ComparePart(@operator.Compare, Common.Helpers.Utils.Database.NEOJ4);

                    leftMap = GetMappedProperty(mapperLinks, left.Value, "neo4j");

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

                        rightMap = GetMappedProperty(mapperLinks, rightTerm.Value, "neo4j");

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

        private IExpression[] GetPropertyPart(BaseExpr.BaseExpr expr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<IExpression> expressions = new List<IExpression>();

            DeclareExpr declareExpr = null;
            DataModelExpr dataModelExpr = null;
            PropertyExpr propertyExpr;
            FunctionExpr functionExpr;

            Properties properties;
            Link mappedProperty;

            if (expr is DeclareExpr)
                declareExpr = (DeclareExpr)expr;
            else if (expr is DataModelExpr)
                dataModelExpr = (DataModelExpr)expr;

            foreach (var part in (declareExpr != null ? declareExpr.Value : dataModelExpr.Value))
            {
                if (part is PropertyExpr)
                {
                    propertyExpr = (PropertyExpr)part;

                    mappedProperty = GetMappedProperty(mapperLinks, propertyExpr.Value, "neo4j");

                    if (mappedProperty != null)
                    {
                        properties = schemas
                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                            .Where(x => x.Property == mappedProperty.Property)
                            .First();

                        if(string.IsNullOrEmpty(mappedProperty.Reference_Property))
                            expressions.Add(new PropertyPart(properties, mappedProperty));
                        else
                            expressions.Add(new UnwindPropertyPart(properties, mappedProperty));

                        expressions.Add(new SeparatorPart(","));
                    };
                }
                else if (part is FunctionExpr)
                {
                    functionExpr = (FunctionExpr)part;

                    foreach (var func in functionExpr.Value)
                    {
                        propertyExpr = (PropertyExpr)func;

                        mappedProperty = GetMappedProperty(mapperLinks, propertyExpr.Value, "neo4j");

                        if (mappedProperty != null)
                        {
                            properties = schemas
                                        .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                        .Where(x => x.Property == mappedProperty.Property)
                                        .First();

                            if (string.IsNullOrEmpty(mappedProperty.Reference_Property))
                            {
                                expressions.Add(new FunctionPart(
                                                new PropertyPart(properties, mappedProperty),
                                                functionExpr.Type));
                            }
                            else
                            {
                                expressions.Add(new FunctionPart(
                                             new UnwindPropertyPart(properties, mappedProperty),
                                             functionExpr.Type));
                            }

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

                    operatorPart = new OperatorPart(@operator.Operator, Common.Helpers.Utils.Database.NEOJ4);

                    leftMap = GetMappedProperty(mapperLinks, left.Value, "neo4j");

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

                        rightMap = GetMappedProperty(mapperLinks, rightTerm.Value, "neo4j");

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

                    operatorPart = new OperatorPart(@operator.Operator, Common.Helpers.Utils.Database.NEOJ4);
                    left = (TermExpr)@operator.Left;
                    
                    leftMap = GetMappedProperty(mapperLinks, left.Value, "neo4j");

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

                        rightMap = GetMappedProperty(mapperLinks, rightTerm.Value, "neo4j");

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
                        insertValueParts.Add(new InsertNodePart(leftPart, operatorPart, rightPart));

                        if (propertiesExpr != null)
                            insertValueParts.Add(new SeparatorPart(","));
                    }
                }
            }

            return insertValueParts.ToArray();
        }
    }
}
