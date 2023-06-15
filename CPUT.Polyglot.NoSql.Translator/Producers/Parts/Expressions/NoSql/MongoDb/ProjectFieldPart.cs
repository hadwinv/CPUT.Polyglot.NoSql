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
        internal string Property { get; set; }

        internal string Alias { get; set; }

        internal bool IsFirstKey { get; set; }

        public ProjectFieldPart(LinkedProperty mappedProperty, bool isFunctionTarget = false)
        {
            if (mappedProperty != null)
            {
                if(isFunctionTarget && mappedProperty.Type == typeof(JsonExpr))
                {
                    Property = mappedProperty.Link.Property.Substring(0, mappedProperty.Link.Property.LastIndexOf("."));
                    Alias = !string.IsNullOrEmpty(mappedProperty.AliasName) ? mappedProperty.AliasName
                                        : Property.Substring(Property.IndexOf(".") + 1);
                   
                }
                else
                {
                    if (mappedProperty.Type == typeof(JsonExpr))
                    {
                        Property = mappedProperty.Link.Property;
                        Alias = !string.IsNullOrEmpty(mappedProperty.AliasName) ? mappedProperty.AliasName
                                        : mappedProperty.Link.Property.Split(".")[mappedProperty.Link.Property.Split(".").Count() - 1];
                    }
                    else
                    {
                        Property = mappedProperty.Link.Property;
                        Alias = mappedProperty.Link.Property.IndexOf(".") > -1
                                    ? mappedProperty.Link.Property.Split(".")[mappedProperty.Link.Property.Split(".").Count() - 1]
                                    : !string.IsNullOrEmpty(mappedProperty.AliasName)
                                        ? mappedProperty.AliasName
                                        : mappedProperty.Link.Property;
                    }
                }
                
            }
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
