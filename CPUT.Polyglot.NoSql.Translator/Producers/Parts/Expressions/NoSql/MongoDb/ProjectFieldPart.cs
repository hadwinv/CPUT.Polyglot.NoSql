using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using System.Xml.Linq;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class ProjectFieldPart : IExpression
    {
        internal string Name { get; set; }

        internal string Alias { get; set; }

        internal string Source { get; set; }

        internal bool Ignore { get; set; }

        public ProjectFieldPart(LinkedProperty mappedProperty, bool isFunctionTarget = false)
        {
            if (!string.IsNullOrEmpty(mappedProperty.Link.Property))
            {
                if (isFunctionTarget && mappedProperty.Type == typeof(JsonExpr))
                {
                    Name = mappedProperty.Link.Property.Substring(0, mappedProperty.Link.Property.LastIndexOf("."));
                    Alias = !string.IsNullOrEmpty(mappedProperty.AliasName) ? mappedProperty.AliasName
                                        : Name.Substring(Name.IndexOf(".") + 1);
                }
                else
                {
                    if (mappedProperty.Type == typeof(JsonExpr))
                    {
                        Name = mappedProperty.Link.Property;

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
