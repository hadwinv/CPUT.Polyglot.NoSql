using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class FunctionFieldPart : IExpression
    {
        internal string Name { get; set; }

        internal string Alias { get; set; }

        internal string Source { get; set; }

        internal bool Ignore { get; set; }

        public FunctionFieldPart(LinkedProperty mappedProperty)
        {
            if (!string.IsNullOrEmpty(mappedProperty.Link.Property))
            {
                if (mappedProperty.Type == typeof(JsonExpr))
                {
                    if (mappedProperty.Link.Property.IndexOf(".") > -1)
                        Name = mappedProperty.Link.Property.Substring(mappedProperty.Link.Property.IndexOf(".") + 1);
                    else
                        Name = mappedProperty.Link.Reference + "." + mappedProperty.Link.Property;

                    if (!string.IsNullOrEmpty(mappedProperty.AliasName))
                        Alias = mappedProperty.AliasName;
                    else
                    {
                        var parts = Name.Split('.');

                        var identifier = string.Empty;

                        for (var i = 0; i <= parts.Length - 1; i++)
                        {
                            if (i == parts.Length - 1)
                                identifier += parts[i];
                            else
                                identifier += parts[i].Substring(0, 1) + "_";
                        }
                        Alias = identifier;
                    }
                }
                else
                {
                    Name = mappedProperty.Link.Property;

                    Alias = !string.IsNullOrEmpty(mappedProperty.AliasName)
                                    ? mappedProperty.AliasName
                                    : Name;
                }
            }
            else
                Ignore = true;

            Source = mappedProperty.Property;
        }

        public void Accept(INeo4jVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(ICassandraVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IRedisVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
