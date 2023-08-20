using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared
{
    public class PropertyPart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal string AliasName { get; set; }

        internal string Type { get; set; }

        internal string Source { get; set; }

        internal bool Ignore { get; set; }

        public PropertyPart(LinkedProperty mappedProperty)
        {
            if (!string.IsNullOrEmpty(mappedProperty.Link.Property))
            {
                if (mappedProperty.Link.Target == Enum.GetName(typeof(Database), Database.NEO4J).ToLower())
                {
                    

                    if (mappedProperty.Link.Property.IndexOf(".") > -1)
                    {
                        var model = Assistor.NSchema[(int)Database.NEO4J].SelectMany(x => x.Model.Where(x => x.Name == mappedProperty.Link.Reference)).First();

                        Name = mappedProperty.Link.Property.Split(".")[mappedProperty.Link.Property.Split(".").Count() - 1];

                        if (model.Type == "array")
                            AliasIdentifier = model.Name.Substring(0, 3);
                        else
                            AliasIdentifier = mappedProperty.Link.Property.Split(".")[0].Substring(0, 3).ToLower();
                    }
                    else
                    {
                        Name = mappedProperty.Link.Property;
                        AliasIdentifier = mappedProperty.Link.Reference.Substring(0, 4).ToLower();
                    }
                }
                else
                {
                    if (mappedProperty.Type == typeof(JsonExpr))
                    {
                        Name = mappedProperty.Link.Property;

                        if (!string.IsNullOrEmpty(mappedProperty.AliasName))
                            AliasName = mappedProperty.AliasName;

                        if (mappedProperty.AggregateType is not null)
                        {
                            var parts = Name.Split('.');

                            var alias = string.Empty;

                            if (parts.Length > 0)
                            {
                                for (var i = 0; i <= parts.Length - 1; i++)
                                {
                                    if (i == parts.Length - 1)
                                        alias += parts[i];
                                    else
                                        alias += parts[i].Substring(0, 1) + "_";
                                }
                                AliasName = alias;
                            }
                        }
                        
                    }
                    else
                    {
                        Name = mappedProperty.Link.Property;
                        AliasName = !string.IsNullOrEmpty(mappedProperty.AliasName)
                                        ? mappedProperty.AliasName
                                        : Name;
                    }
                }

                //if (string.IsNullOrEmpty(AliasIdentifier))
                //    AliasIdentifier = mappedProperty.Link.Reference.Substring(0, 3).ToLower();
            }
            else
                Ignore = true;

            Source = mappedProperty.Property;
        }

        public PropertyPart(BaseExpr baseExpr)
        {
            dynamic? expr = baseExpr is StringLiteralExpr ? ((StringLiteralExpr)baseExpr) : ((NumberLiteralExpr)baseExpr);

            Name = expr is StringLiteralExpr ? expr.Value : expr.Value.ToString();
            Type = expr is StringLiteralExpr ? "string" : "int";
        }

        public void Accept(INeo4jVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(ICassandraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(IRedisVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
