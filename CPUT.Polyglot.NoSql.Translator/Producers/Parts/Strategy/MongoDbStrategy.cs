using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.Text;
using BaseExpr = CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using Component = CPUT.Polyglot.NoSql.Parser.Syntax.Component;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class MongoDbStrategy : StrategyPart
    {
        public override string Alter()
        {
            //            --Add the join_date column in employees collection.
            //db.employees.update(
            //{ },
            //{ $set: { join_date: "" } },
            //{ multi: true }
            //)
            //            --Remove the join_date column from employees table
            //db.employees.update(
            //{ },
            //{ $unset: { join_date: "" } },
            //{ multi: true }
            //)
            throw new NotImplementedException();
        }

        public override string Create()
        {
//            db.employees.insert( {
//            emp_id: "RAM",
//age: 50,
//status: "A"
//} )

//Or

//db.createCollection("employees");
            throw new NotImplementedException();
        }

        public override string Describe()
        {
            throw new NotImplementedException();
        }

        public override string Fetch(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting MongoDbPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToFindModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise mongodb query generator
            var generator = new MongoDbGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Modify(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting MongoDbPart - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToModifyModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise mongodb query generator
            var generator = new MongoDbGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Add(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting MongoDbPart - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToAddModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise mongodb query generator
            var generator = new MongoDbGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        private List<IExpression> ConvertToFindModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> queryParts = new List<IExpression>();
            List<IExpression> logicalParts = new List<IExpression>();
            List<IExpression> propertyParts = new List<IExpression>();

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DataModelExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();

            //get collection(s)
            targetModel.AddRange(GetCollectionPart(DataModelExpr, mapperLinks));

            if (targetModel.Count > 0)
            {
                if (FilterExpr != null)
                    logicalParts.AddRange(GetLogicalPart(FilterExpr, mapperLinks, schemas));

                //get property fields
                propertyParts.AddRange(GetPropertyPart(DeclareExpr, mapperLinks, schemas));

                if (propertyParts.Count > 0)
                    propertyParts.RemoveAt(propertyParts.Count - 1);

                //add detailed logic
                if (logicalParts.Count > 0)
                    queryParts.Add(new ConditionPart(logicalParts.ToArray()));

                queryParts.Add(new FieldPart(propertyParts.ToArray()));

                targetModel.Add(new FindPart(queryParts.ToArray()));

                if (this.OrderByExpr != null)
                {
                    var mappedProperty = GetMappedProperty(mapperLinks, this.OrderByExpr, "mongodb");

                    if (mappedProperty != null)
                        targetModel.Add(new OrderByPart(mappedProperty, this.OrderByExpr));
                }

                if (this.RestrictExpr != null)
                    targetModel.Add(new RestrictPart(this.RestrictExpr.Value));
            }

            return targetModel;
        }

        private List<IExpression> ConvertToModifyModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> queryParts = new List<IExpression>();
            List<IExpression> logicalParts = new List<IExpression>();
            List<IExpression> setParts = new List<IExpression>();

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DeclareExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();

            //get collection(s)
            targetModel.AddRange(GetCollectionPart(DeclareExpr, mapperLinks));

            if (targetModel.Count > 0)
            {
                if (FilterExpr != null)
                    logicalParts.AddRange(GetLogicalPart(FilterExpr, mapperLinks, schemas));

                //set property fields
                setParts.AddRange(GetSetValuePart(PropertiesExpr, mapperLinks, schemas));

                if (setParts.Count > 0)
                    setParts.RemoveAt(setParts.Count - 1);

                //add detailed logic
                if (logicalParts.Count > 0)
                    queryParts.Add(new ConditionPart(logicalParts.ToArray()));

                queryParts.Add(new SetPart(setParts.ToArray()));

                targetModel.Add(new UpdatePart(queryParts.ToArray()));
            }

            return targetModel;
        }

        private List<IExpression> ConvertToAddModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            List<IExpression> queryParts = new List<IExpression>();
            List<IExpression> addParts = new List<IExpression>();

            //get all linked properties, model, etc
            var mapperLinks = mapper
                  .Where(x => DeclareExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();

            //get collection(s)
            targetModel.AddRange(GetCollectionPart(DeclareExpr, mapperLinks));

            if (targetModel.Count > 0)
            {
                //set property fields
                addParts.AddRange(GetSetValuePart(PropertiesExpr, mapperLinks, schemas));

                if (addParts.Count > 0)
                    addParts.RemoveAt(addParts.Count - 1);

                queryParts.Add(new AddPart(addParts.ToArray()));

                targetModel.Add(new InsertPart(queryParts.ToArray()));
            }

            return targetModel;
        }

        #region Parts

        private GetCollectionPart[] GetCollectionPart(BaseExpr.BaseExpr expr, List<MappedSource> mapperLinks)
        {
            List<GetCollectionPart> collectionParts = new List<GetCollectionPart>();
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
                        .Where(t => t.Target == "mongodb")
                        .FirstOrDefault();

                    if (map != null)
                        collectionParts.Add(new GetCollectionPart(map.Reference));
                }
            }

            return collectionParts.ToArray();
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

                    operatorPart = new OperatorPart(@operator.Operator, Common.Helpers.Utils.Database.MONGODB);
                    comparePart = new ComparePart(@operator.Compare, Common.Helpers.Utils.Database.MONGODB);

                    leftMap = GetMappedProperty(mapperLinks, left, "mongodb");

                    if (leftMap != null)
                    {
                        properties = schemas
                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                            .First(x => x.Property == leftMap.Property);

                        leftPart = new PropertyPart(properties, leftMap, left);
                    }

                    if (@operator.Right is TermExpr)
                    {
                        var right = (TermExpr)@operator.Right;

                        rightMap = GetMappedProperty(mapperLinks, right, "mongodb");

                        if (rightMap != null)
                        {
                            properties = schemas
                                .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                .First(x => x.Property == rightMap.Property);

                            rightPart = new PropertyPart(properties, rightMap, right);
                        }
                    }
                    else if (@operator.Right is StringLiteralExpr)
                        rightPart = new PropertyPart((StringLiteralExpr)@operator.Right);
                    else if (@operator.Right is NumberLiteralExpr)
                        rightPart = new PropertyPart((NumberLiteralExpr)@operator.Right);

                    if(leftPart != null && rightPart != null)
                        logicalParts.Add(new LogicalPart(leftPart, operatorPart, rightPart, comparePart));
                }
            }

            return logicalParts.ToArray();
        }

        private IExpression[] GetSetValuePart(BaseExpr.BaseExpr expr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<IExpression> setValueParts = new List<IExpression>();
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

                    operatorPart = new OperatorPart(@operator.Operator, Common.Helpers.Utils.Database.MONGODB);

                    leftMap = GetMappedProperty(mapperLinks, left, "mongodb");

                    if (leftMap != null)
                    {
                        properties = schemas
                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                            .First(x => x.Property == leftMap.Property);

                        leftPart = new PropertyPart(properties, leftMap, left);
                    }

                    if (@operator.Right is TermExpr)
                    {
                        var right = (TermExpr)@operator.Right;

                        rightMap = GetMappedProperty(mapperLinks, right, "mongodb");

                        if (rightMap != null)
                        {
                            properties = schemas
                                .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                .First(x => x.Property == rightMap.Property);

                            rightPart = new PropertyPart(properties, rightMap, right);
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

        private IExpression[] GetPropertyPart(BaseExpr.BaseExpr expr, List<MappedSource> mapperLinks, List<NSchema> schemas)
        {
            List<IExpression> expressions = new List<IExpression>();
            PropertyPart propertyPart = null;

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

                    mappedProperty = GetMappedProperty(mapperLinks, propertyExpr, "mongodb");

                    if (mappedProperty != null)
                    {
                        properties = schemas
                            .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                            .Where(x => x.Property == mappedProperty.Property)
                            .First();

                        expressions.Add(new PropertyPart(properties, mappedProperty, propertyExpr));
                        expressions.Add(new SeparatorPart(","));
                    };
                }
                else if (part is FunctionExpr)
                {
                    functionExpr = (FunctionExpr)part;

                    foreach (var func in functionExpr.Value)
                    {
                        propertyExpr = (PropertyExpr)func;

                        mappedProperty = GetMappedProperty(mapperLinks, propertyExpr, "mongodb"); 

                        if (mappedProperty != null)
                        {
                            properties = schemas
                                        .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                        .Where(x => x.Property == mappedProperty.Property)
                                        .First();

                            expressions.Add(new PropertyPart(properties, mappedProperty, propertyExpr));
                            expressions.Add(new SeparatorPart(","));
                        }
                    }
                }
            }

            return expressions.ToArray();
        }
        
        #endregion
    }
}