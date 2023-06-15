﻿using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class GroupByFieldPart : IExpression
    {
        internal string Property { get; set; }

        internal string Alias { get; set; }

        //BaseExpr baseExpr, Link mapping, int target
        public GroupByFieldPart(LinkedProperty mappedProperty)
        {
            if (mappedProperty.Type == typeof(JsonExpr))
            {
                Property = mappedProperty.Link.Property.Split(".")[mappedProperty.Link.Property.Split(".").Count() - 1];

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

            //Property = mappedProperty.Link.Property;
            //Alias = mappedProperty.Link.Property.IndexOf(".") > -1
            //? mappedProperty.Link.Property.Split(".")[mappedProperty.Link.Property.Split(".").Count() - 1] :
            //!string.IsNullOrEmpty(mappedProperty.AliasName) ? mappedProperty.AliasName : mappedProperty.Link.Property;

            //Property = mapping.Property;
            //Alias = Property;

            //dynamic? expr = baseExpr is PropertyExpr ? ((PropertyExpr)baseExpr) :
            //                baseExpr is TermExpr ? ((TermExpr)baseExpr) :
            //                baseExpr is JsonExpr ? ((JsonExpr)baseExpr) : default;

            //if (expr != null)
            //{
            //    if (expr is JsonExpr)
            //    {
            //        var child = Assistor.NSchema[target].SelectMany(x => x.Model.Where(x => x.Name == mapping.Reference)).FirstOrDefault();

            //        if (child != null)
            //        {
            //            var unwindProperty = Assistor.UnwindProperty(child, target);

            //            var path = unwindProperty.Split(".");

            //            Property = path[path.Length - 1] + "." + mapping.Property;
            //            Alias = mapping.Property;
            //        }
            //    }
            //}
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
