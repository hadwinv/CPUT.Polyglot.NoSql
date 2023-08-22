using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions;
using System.Text;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using Pipelines.Sockets.Unofficial.Arenas;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using System.Collections.Specialized;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Models.Views.Unified;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using App.Metrics;
using App.Metrics.Timer;
using CPUT.Polyglot.NoSql.Common.Reporting;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public class RedisStrategy : StrategyPart
    {
        protected string Target = "redis";

        public RedisStrategy(){}

        public override OutputPart Fetch()
        {
            Console.WriteLine("Starting RedisPart - Fetch");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToGetModel();

            if (targetQuery.Count() > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise redis query generator
                var generator = new RedisGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }

            return new OutputPart
            {
                Query = query.ToString(),
                Codex = BuildCodex(targetQuery)
            };
        }

        public override OutputPart Modify()
        {
            var output = new OutputPart();

            Console.WriteLine("Starting RedisPart - Modify");

            var query = new StringBuilder();

            //set expression parts
            var targetQuery = ConvertToGetSetModel();

            if (targetQuery.Count() > 0)
            {
                //pass query expresion
                var match = new QueryPart(targetQuery.ToArray());

                //initialise redis query generator
                var generator = new RedisGenerator(query);

                //kick off visitors
                match.Accept(generator);
            }

            Console.WriteLine(query);

            output.Query = query.ToString();

            return output;
        }

        public override OutputPart Add()
        {
            var output = new OutputPart();

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

            Console.WriteLine(query);

            output.Query = query.ToString();

            return output;
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

                            if(leftPart != null)
                            {
                                //check if property is used as key
                                if (Assistor.NSchema[(int)Database.REDIS]
                                    .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                    .FirstOrDefault(x => x.Property == leftPart.Name && x.Key) != null)
                                {
                                    var native = Assistor.NSchema[(int)Database.REDIS]
                                                .SelectMany(x => x.Model)
                                                .Where(x => x.Properties.Exists(x => x.Property == leftPart.Name && x.Key))
                                                .First();

                                    var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                                    if(rightPart != null)
                                        parts.Add(new GetPart(native.Name, rightPart));
                                }
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
                    parts.Add(new SetKeyValuePart(((GetPart)part).Property.Name, GetSetModel()));
                else if (part is KeyPart)
                    parts.Add(new SetKeyValuePart("{0}", GetSetModel()));
                    
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

                                if (leftPart != null)
                                {
                                    //check if property is used as key
                                    if (Assistor.NSchema[(int)Database.REDIS]
                                        .SelectMany(x => x.Model.SelectMany(x => x.Properties))
                                        .FirstOrDefault(x => x.Property == leftPart.Name && x.Key) != null)
                                    {
                                        var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                                        if(rightPart != null)
                                            parts.Add(new SetKeyValuePart(rightPart.Name, GetSetModel()));

                                        break;
                                    }
                                }
                                    
                            }
                        }
                    }
                }
            }

            return parts;
        }

        public List<string> GetSetModel()
        {
            var parts = new List<string>();

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
                                var rightPart = LeftRightPart(operatorExpr, DirectionType.Right, Target);

                                if(leftPart != null && rightPart != null)
                                    parts.Add(leftPart.Name + "="+ rightPart.Name);
                            }
                        }
                    }
                }
            }

            return parts;
        }

        private PropertyPart? LeftRightPart(OperatorExpr operatorExpr, DirectionType direction, string database)
        {
            if (DirectionType.Left == direction)
            {
                var left = GetMappedProperty(operatorExpr.Left, database);

                if (left.Link != null)
                    return new PropertyPart(left);
            }

            else
            {
                if (operatorExpr.Right is PropertyExpr || operatorExpr.Right is TermExpr || operatorExpr.Right is JsonExpr)
                {
                    var right = GetMappedProperty(operatorExpr.Right, database);

                    if (right.Link != null)
                        return new PropertyPart(right);
                }
                else
                    return new PropertyPart(operatorExpr.Right);
            }

            return default;
        }

        private LinkedProperty GetMappedProperty(BaseExpr baseExpr, string database)
        {
            LinkedProperty mappedProperty = new LinkedProperty();

            dynamic? expr = baseExpr is PropertyExpr ? ((PropertyExpr)baseExpr) :
                            baseExpr is TermExpr ? ((TermExpr)baseExpr) :
                            baseExpr is OrderByPropertyExpr ? ((OrderByPropertyExpr)baseExpr) :
                            baseExpr is JsonExpr ? ((JsonExpr)baseExpr) : default;

            if (expr != null)
            {
                mappedProperty.Type = baseExpr.GetType();
                mappedProperty.Property = expr.Value;
                mappedProperty.AliasIdentifier = expr.AliasIdentifier;

                if (expr.GetType().GetProperty("AliasName") != null)
                    mappedProperty.AliasName = expr.AliasName;

                if (DataModelExpr != null)
                {
                    DataExpr dataExpr;

                    if (!string.IsNullOrEmpty(mappedProperty.AliasIdentifier))
                        dataExpr = DataModelExpr.Value.Cast<DataExpr>().ToList().Single(x => x.AliasIdentifier == mappedProperty.AliasIdentifier);
                    else
                        dataExpr = DataModelExpr.Value.Cast<DataExpr>().First();

                    if (mappedProperty.Property.IndexOf('.') > -1)
                    {
                        var iterator = 0;

                        Resources resource = null;
                        Resources prevresource = null;

                        mappedProperty.Link = new Link();

                        var @base = Assistor.USchema.Single(x => x.View.Name == dataExpr.Value);

                        var target = (Database)Enum.Parse(typeof(Database), database.ToUpper());

                        //get link references based on the data model
                        var references = @base.View.Resources
                                        .Where(x => x.Property == "base")
                                        .SelectMany(x => x.Link.Where(x => x.Target == database))
                                        .First();

                        //get all linked properties, model, etc
                        var native = Assistor.NSchema[(int)target]
                            .SelectMany(s => s.Model)
                            .Where(x => x.Name == references.Reference)
                            .FirstOrDefault();

                        foreach (var reference in mappedProperty.Property.Split('.'))
                        {
                            if (iterator == 0)
                            {
                                //set base reference i.e first reference in json path
                                resource = @base.View.Resources.Single(x => x.Property == reference);
                            }
                            else
                            {
                                @base = Assistor.USchema.Single(x => x.View.Name == prevresource?.Type);

                                resource = @base.View.Resources.Single(x => x.Property == reference);
                            }

                            if (resource.Metadata != "class")
                            {
                                var link = SearchAndFindPropertyLink(prevresource?.Property, reference, database);

                                if (link != null)
                                {
                                    var field = Assistor.NSchema[(int)target]
                                        .SelectMany(x => x.Model.Where(x => x.Name == link.Reference))
                                        .SelectMany(x => x.Properties.Where(x => x.Property == link.Property))
                                        .First();

                                    //find root 
                                    var path = GetPropertyPath(native.Name, link.Reference, target);

                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        if (path != dataExpr.Value)
                                            mappedProperty.Link.Property = path + "." + field.Property;
                                        else
                                            mappedProperty.Link.Property = field.Property;
                                    }
                                    else
                                        mappedProperty.Link.Property = field.Property;

                                    mappedProperty.Link.Reference = link.Reference;
                                    mappedProperty.Link.Target = link.Target;
                                    mappedProperty.SourceReference = @base.View.Name;

                                }
                                else
                                    mappedProperty.Link = default;
                            }

                            prevresource = resource;
                            iterator++;
                        }
                    }
                    else
                    {
                        mappedProperty.Link = SearchAndFindPropertyLink(dataExpr.Value, mappedProperty.Property, database);
                        mappedProperty.SourceReference = dataExpr.Value;
                    }

                }
            }

            return mappedProperty;
        }

        
        private PropertyPart[] GetSelection()
        {
            var parts = new List<PropertyPart>();

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
                            parts.Add(new PropertyPart(property));
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
                                parts.Add(new PropertyPart(property));
                            }
                        }
                    }
                }
            }

            return parts.ToArray();
        }

        private string GetPropertyPath(string @base, string propertyReference, Database target)
        {
            var path = string.Empty;

            var models = Assistor.NSchema[(int)target]
                                      .SelectMany(x => x.Model)
                                      .Where(x => x.Properties.Exists(t => t.Property == propertyReference))
                                      .FirstOrDefault();
            //x.Name == modelReference &&
            //string modelReference,
            if (models != null)
            {
                if (@base == models.Name)
                    path = propertyReference;
                else
                {
                    var test = GetPropertyPath(@base, models.Name, target);

                    if (string.IsNullOrEmpty(path))
                    {
                        if (string.IsNullOrEmpty(test))
                            path = models.Name + "." + propertyReference;
                        else
                            path = test + "." + propertyReference;
                    }

                    else
                        path = path + "." + test;
                }
            }

            return path;

        }

        private Codex BuildCodex(IExpression[] parts)
        {
            FromProperty from;
            ToProperty to;

            var codex = new Codex
            {
                Target = Database.REDIS,
                PropertyModel = new List<Model>(),
                DataModel = DataModelExpr
            };

            foreach (var part in parts)
            {
                if (part is GetPart)
                {
                    var @get = ((GetPart)part);

                    var model = new Model
                    {
                        Name = @get.Source,
                        Views = new Dictionary<FromProperty, ToProperty>()
                    };

                    foreach (var property in GetSelection())
                    {
                        from = new FromProperty
                        {
                            Name = property.Name,
                            Alias = property.AliasName
                        };

                        to = new ToProperty
                        {
                            Name = property.Source,
                        };

                        model.Views.Add(from, to);
                    }

                    codex.PropertyModel.Add(model);
                }
                else if (part is KeyPart)
                {
                    var @get = ((KeyPart)part);

                    var model = new Model
                    {
                        Views = new Dictionary<FromProperty, ToProperty>()
                    };

                    foreach (var property in GetSelection())
                    {
                        from = new FromProperty
                        {
                            Name = property.Name,
                            Alias = property.AliasName
                        };

                        to = new ToProperty
                        {
                            Name = property.Source,
                        };

                        model.Views.Add(from, to);
                    }

                    codex.PropertyModel.Add(model);
                }
            }

            return codex;
        }


        #endregion
    }
}
