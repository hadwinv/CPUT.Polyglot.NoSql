using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using System.Text;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class RedisStrategy : StrategyPart
    {
        protected string Target = "redis";
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

        public override string Fetch()
        {
            Console.WriteLine("Starting RedisPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToGetModel();

            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise redis query generator
            var generator = new RedisGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Modify()
        {
            Console.WriteLine("Starting RedisPart - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToGetSetModel();

          
            //pass query expresion
            var match = new QueryPart(targetQuery.ToArray());

            //initialise redis query generator
            var generator = new RedisGenerator(query);

            //kick off visitors
            match.Accept(generator);

            Console.WriteLine(query);

            return query.ToString();
        }

        public override string Add()
        {
            Console.WriteLine("Starting RedisPart - Add");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToSetModel();

            if (targetQuery.Count > 0)
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

            //Console.WriteLine(query);

            return query.ToString();
        }

        #region Expression Parts

        private IExpression[] ConvertToGetModel()
        {
            var parts = new List<IExpression>();

            if (FilterExpr != null)
            {
                foreach (var part in FilterExpr.Value)
                {
                    if (part is GroupExpr)
                    {
                        var groupExpr = (GroupExpr)part;
                        var operatorExpr = (OperatorExpr)groupExpr.Value;

                        if (operatorExpr.Operator == OperatorType.Eql)
                        {
                            var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);

                            //check if property is used as key
                            if(Assistor.NSchema
                                .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                .FirstOrDefault(x => x.Property == leftPart.Name && x.Key) != null)
                            {
                                var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                                parts.Add(new GetPart(rightPart));
                                break;
                            }
                        }
                    }
                }
            }
            
            if(parts.Count == 0)
                parts.Add(new KeyPart("*"));

            return parts.ToArray();
        }

        private IExpression[] ConvertToGetSetModel()
        {
            var parts = new List<IExpression>();

            var getParts = ConvertToGetModel();

            foreach (var part in getParts)
            {
                parts.Add(part);

                if (part is GetPart)
                    parts.Add(new SetKeyValuePart(((GetPart)part).Property.Name, "{0}"));
                else if (part is KeyPart)
                    parts.Add(new SetKeyValuePart("{0}", "{0}"));
            }

            return parts.ToArray();
        }

        public List<IExpression> ConvertToSetModel()
        {
            var parts = new List<IExpression>();


            if (PropertiesExpr != null)
            {
                foreach (var groups in PropertiesExpr.Value)
                {
                    foreach (var part in ((GroupPropertiesExpr)groups).Value)
                    {
                        if (part is GroupExpr)
                        {
                            var groupExpr = (GroupExpr)part;
                            var operatorExpr = (OperatorExpr)groupExpr.Value;

                            if (operatorExpr.Operator == OperatorType.Eql)
                            {
                                var leftPart = LeftRightPart(operatorExpr, DirectionType.Left, Target);

                                //check if property is used as key
                                if (Assistor.NSchema
                                    .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                    .FirstOrDefault(x => x.Property == leftPart.Name && x.Key) != null)
                                {
                                    var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                                    parts.Add(new SetKeyValuePart(rightPart.Name, "{0}"));
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return parts;
        }

        #endregion
    }
}
