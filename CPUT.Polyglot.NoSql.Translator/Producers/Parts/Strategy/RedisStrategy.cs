using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.Text;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using Cassandra.Mapping;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using System.ComponentModel;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using BaseExpr = CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using Component = CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class RedisStrategy : StrategyPart
    {
        public RedisStrategy() {}

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
            Console.WriteLine("Starting RedisPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToGetModel(expression, mapper, schemas);

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise redis query generator
            var generator = new RedisGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Modify(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting RedisPart - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToGetModel(expression, mapper, schemas);

            if(targetQuery[0] is GetPart)
            {
                var key = ((GetPart)targetQuery[0]).Property.Name;

                targetQuery.Add(new Expressions.NoSql.Redis.SetKeyValuePart(key, "{0}"));
            }
            else if (targetQuery[0] is KeyPart)
                targetQuery.Add(new Expressions.NoSql.Redis.SetKeyValuePart("{0}", "{0}"));
                
            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise redis query generator
            var generator = new RedisGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Add(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            Console.WriteLine("Starting RedisPart - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToSetModel(expression, mapper, schemas);

            if(targetQuery.Count > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise redis query generator
                var generator = new RedisGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }
            else
                query.Append("Cannot generate Redis command");

            Console.WriteLine(query);

            return query.ToString();
        }

        public List<IExpression> ConvertToGetModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();

            PropertyPart propertyPart = null;
            GroupExpr group = null;
            OperatorExpr @operator = null;
            TermExpr left = null;
            Link leftMap = null;
            Properties properties = null;

            List<MappedSource> mapperLinks = null;

            //set expression parts
            var DeclareExpr = (Component.DeclareExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DeclareExpr)));
            var DataModelExpr = (Component.DataModelExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.DataModelExpr)));
            var FilterExpr = (Component.FilterExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.FilterExpr)));
            
            //get all linked properties, model, etc
            if(DeclareExpr.Value.Any(x => x is DataExpr))
            {
                mapperLinks = mapper
                 .Where(x => DeclareExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                 .Select(s => s)
                 .ToList();
            }
            else
            {
                mapperLinks = mapper
                  .Where(x => DataModelExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                  .Select(s => s)
                  .ToList();
            }
             

            if(FilterExpr != null)
            {
                foreach (var part in FilterExpr.Value)
                {
                    if (part is GroupExpr)
                    {
                        group = (GroupExpr)part;
                        @operator = (OperatorExpr)group.Value;
                        left = (TermExpr)@operator.Left;

                        if(@operator.Operator == OperatorType.Eql)
                        {
                            leftMap = GetMappedProperty(mapperLinks, left, "redis");

                            if (leftMap != null)
                            {
                                //check if property is used as key
                                properties = schemas
                                    .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                    .FirstOrDefault(x => x.Property == leftMap.Property && x.Key);

                                if (properties != null)
                                {
                                    if (@operator.Right is StringLiteralExpr)
                                        propertyPart = new PropertyPart((StringLiteralExpr)@operator.Right);
                                    else if (@operator.Right is NumberLiteralExpr)
                                        propertyPart = new PropertyPart((NumberLiteralExpr)@operator.Right);

                                    if (propertyPart != null)
                                        targetModel.Add(new GetPart(propertyPart));
                                }
                            }
                        }
                    }
                }

                if(targetModel.Count == 0)  
                    targetModel.Add(new KeyPart("*"));
            }
            else
            {
                targetModel.Add(new KeyPart("*"));
            }

            return targetModel;
        }

        public List<IExpression> ConvertToSetModel(BaseExpr.BaseExpr expression, List<MappedSource> mapper, List<NSchema> schemas)
        {
            List<IExpression> targetModel = new List<IExpression>();
            PropertyPart propertyPart = null;

            GroupExpr group = null;
            OperatorExpr @operator;
            TermExpr left;
            Link leftMap;
            Properties properties;

            //set expression parts
            var DeclareExpr = (Component.DeclareExpr)expression.ParseTree.Single(x => x.GetType().Equals(typeof(Component.DeclareExpr)));
            var PropertiesExpr = (Component.PropertiesExpr?)expression.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(Component.PropertiesExpr)));

            var mapperLinks = mapper
                 .Where(x => DeclareExpr.Value.Select(x => ((DataExpr)x).Value).ToList().Contains(x.Name))
                 .Select(s => s)
                 .ToList();

            foreach (var part in PropertiesExpr.Value)
            {
                if (part is GroupExpr)
                {
                    group = (GroupExpr)part;
                    @operator = (OperatorExpr)group.Value;
                    left = (TermExpr)@operator.Left;

                    if (@operator.Operator == OperatorType.Eql)
                    {
                        leftMap = GetMappedProperty(mapperLinks, left, "redis");

                        if (leftMap != null)
                        {
                            //check if property is used as key
                            properties = schemas
                                .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                .FirstOrDefault(x => x.Property == leftMap.Property && x.Key);

                            if (properties != null)
                            {
                                if (@operator.Right is StringLiteralExpr)
                                    propertyPart = new PropertyPart((StringLiteralExpr)@operator.Right);
                                else if (@operator.Right is NumberLiteralExpr)
                                    propertyPart = new PropertyPart((NumberLiteralExpr)@operator.Right);

                                if (propertyPart != null)
                                {
                                    targetModel.Add(new Expressions.NoSql.Redis.SetKeyValuePart(propertyPart.Name, "{0}"));
                                    break;
                                }
                                    
                            }
                        }
                    }
                }
            }

            return targetModel;
        }

    }
}
