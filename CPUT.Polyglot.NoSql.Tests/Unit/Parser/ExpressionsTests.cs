using CPUT.Polyglot.NoSql.Parser;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using FluentAssertions;
using NUnit.Framework;
using Superpower;
using System.Linq;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Parser
{
    [TestFixture]
    public class ExpressionsTests
    {
        [Test]
        public void Fetch_SimpleQuery_ReturnSyntaxStructure()
        {
            var input = @" FETCH { property }
                            DATA_MODEL { data }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Select.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DeclareExpr),
                //data model
                typeof(DataModelExpr),
                //restrict model
                typeof(RestrictExpr),
                //target model
                typeof(TargetExpr)
            );
        }

        [Test]
        public void Fetch_SimpleQueryWithAllAlias_ReturnSyntaxStructure()
        {
            var input = @" FETCH { d.property AS myproperty, d.property AS myproperty}
                            DATA_MODEL { data AS d }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Select.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DeclareExpr),
                //data model
                typeof(DataModelExpr),
                //restrict model
                typeof(RestrictExpr),
                //target model
                typeof(TargetExpr)
            );
        }

        [Test]
        public void Fetch_SimpleQueryWithPartialAlias_ReturnSyntaxStructure()
        {
            var input = @" FETCH { d.property AS myproperty,  d.property}
                            DATA_MODEL { data AS d }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Select.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DeclareExpr),
                //data model
                typeof(DataModelExpr),
                //restrict model
                typeof(RestrictExpr),
                //target model
                typeof(TargetExpr)
            );
        }


        [Test]
        public void Fetch_QueryWithCondition_ReturnSyntaxStructure()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo = propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Select.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DeclareExpr),
                //data model
                typeof(DataModelExpr),
                //filter
                typeof(FilterExpr),
                //restrict model
                typeof(RestrictExpr),
                //target model
                typeof(TargetExpr)
                );
        }

        [Test]
        public void Fetch_QueryWithMoreThanOneEntity_ReturnSyntaxStructure()
        {
            var input = @"FETCH { property, property }
                            DATA_MODEL { data, data}
                            FILTER_ON {filter = 2 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Select.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DeclareExpr),
                //data model
                typeof(DataModelExpr),
                //filter
                typeof(FilterExpr),
                //target model
                typeof(TargetExpr)
                );
        }

        [Test]
        public void Fetch_ComplexQuery_ReturnSyntaxStructure()
        {
            var input = @"FETCH { property, property, SUM(property) }
                            DATA_MODEL { data, data}
                            FILTER_ON {filter = 2 AND filter = 'unit' }
                            RESTRICT_TO { 10 }
                            ORDER_BY {property DESC}
                            TARGET { storage_type, storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Select.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DeclareExpr),
                //data model
                typeof(DataModelExpr),
                //filter
                typeof(FilterExpr),
                //restrict
                typeof(RestrictExpr),
                //order
                typeof(OrderByExpr),
                //target model
                typeof(TargetExpr)
                );
        }

        [Test]
        public void Fetch_ComplexQueryWithAlias_ReturnSyntaxStructure()
        {
            var input = @"FETCH { a.property AS p, a.property AS p, SUM(b.property) }
                          DATA_MODEL { data AS a, data AS b}
                          FILTER_ON {a.filter = 2 AND b.filter = 'unit' }
                          RESTRICT_TO { 10 }
                          ORDER_BY { a.property DESC }
                          TARGET { storage_type, storage_type }";

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Select.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DeclareExpr),
                //data model
                typeof(DataModelExpr),
                //filter
                typeof(FilterExpr),
                //restrict
                typeof(RestrictExpr),
                //order
                typeof(OrderByExpr),
                //target model
                typeof(TargetExpr)
                );
        }

        [Test]
        public void Add_SingleRow_ReturnSyntaxStructure()
        {
            var input = @" ADD { Entity }
                           PROPERTIES { colum = '1' }
                           TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Insert.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DataModelExpr),
                //properties
                typeof(PropertiesExpr),
                //target
                typeof(TargetExpr)
            );
        }

        [Test]
        public void Add_MoreThanOneRow_ReturnSyntaxStructure()
        {
            var input = @" ADD { Entity }
                           PROPERTIES {[ {column1 = '1'}, {column2 = '2'} ]}
                           TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Insert.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DataModelExpr),
                //properties
                typeof(PropertiesExpr),
                //target
                typeof(TargetExpr)
            );
        }

        [Test]
        public void Modify_SingleRow_ReturnSyntaxStructure()
        {
            var input = @" MODIFY { Entity }
                           PROPERTIES { colum = '1' }
                           TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            var syntax = Expressions.Update.Parse(tokens);

            syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
                //fetch
                typeof(DataModelExpr),
                //properties
                typeof(PropertiesExpr),
                //target
                typeof(TargetExpr)
            );
        }

        //[Test]
        //public void Add_MoreThanOneRow_ReturnSyntaxStructure()
        //{
        //    var input = @" ADD { Entity }
        //                   PROPERTIES { colum = '1', colum = 2 }
        //                   DATA_MODEL { data, data}
        //                   LINK_ON { property = property }    
        //                   FILTER_ON { (propertyo = propertyo) }
        //                   TARGET { storage_type }";

        //    var tokens = new Lexer().Tokenize(input);

        //    var syntax = Expressions.Insert.Parse(tokens);

        //    syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
        //        //fetch
        //        typeof(DeclareExpr),
        //        //properties
        //        typeof(PropertiesExpr),
        //        //data model
        //        typeof(DataModelExpr),
        //        //link
        //        typeof(LinkExpr),
        //        //filter
        //        typeof(FilterExpr),
        //        //target model
        //        typeof(TargetExpr)
        //        );
        //}

        //[Test]
        //public void Add_MoreThanOneRowWithAggregate_ReturnSyntaxStructure()
        //{
        //    var input = @" ADD { Entity }
        //                   PROPERTIES { colum = '1', column = sum(column) }
        //                   DATA_MODEL { data, data}
        //                   LINK_ON { property = property }    
        //                   FILTER_ON { (propertyo = propertyo) }
        //                   TARGET { storage_type }";

        //    var tokens = new Lexer().Tokenize(input);

        //    var syntax = Expressions.Insert.Parse(tokens);

        //    syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
        //        //fetch
        //        typeof(DeclareExpr),
        //        //properties
        //        typeof(PropertiesExpr),
        //        //data model
        //        typeof(DataModelExpr),
        //        //link
        //        typeof(LinkExpr),
        //        //filter
        //        typeof(FilterExpr),
        //        //target model
        //        typeof(TargetExpr)
        //        );
        //}

        //[Test]
        //public void Add_MoreThanOneRowWithCondition_ReturnSyntaxStructure()
        //{
        //    var input = @" ADD { Entity }
        //                   PROPERTIES { colum = '1', column = sum(column) }
        //                   DATA_MODEL { data, data}
        //                   LINK_ON { property = property AND property = property OR property = property }    
        //                   FILTER_ON { (propertyo = propertyo) }
        //                   TARGET { storage_type }";

        //    var tokens = new Lexer().Tokenize(input);

        //    var syntax = Expressions.Insert.Parse(tokens);

        //    syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
        //        //fetch
        //        typeof(DeclareExpr),
        //        //properties
        //        typeof(PropertiesExpr),
        //        //data model
        //        typeof(DataModelExpr),
        //        //link
        //        typeof(LinkExpr),
        //        //filter
        //        typeof(FilterExpr),
        //        //target model
        //        typeof(TargetExpr)
        //        );
        //}

        //[Test]
        //public void Add_MoreThanOneRowWithRestriction_ReturnSyntaxStructure()
        //{
        //    var input = @" ADD { Entity }
        //                   PROPERTIES { colum = '1', column = sum(column) }
        //                   DATA_MODEL { data, data}
        //                   LINK_ON { property = property AND property = property OR property = property }    
        //                   FILTER_ON { (propertyo = propertyo) }
        //                   RESTRICT_TO { 1 }
        //                   TARGET { storage_type }";

        //    var tokens = new Lexer().Tokenize(input);

        //    var syntax = Expressions.Insert.Parse(tokens);

        //    syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
        //        //fetch
        //        typeof(DeclareExpr),
        //        //properties
        //        typeof(PropertiesExpr),
        //        //data model
        //        typeof(DataModelExpr),
        //        //link
        //        typeof(LinkExpr),
        //        //filter
        //        typeof(FilterExpr),
        //        //restrict
        //        typeof(RestrictExpr),
        //        //target model
        //        typeof(TargetExpr)
        //        );
        //}



        //[Test]
        //public void Modify_MoreThanOneRow_ReturnSyntaxStructure()
        //{
        //    var input = @" MODIFY { Entity }
        //                   PROPERTIES { colum = '1', colum = 2 }
        //                   DATA_MODEL { data, data}
        //                   LINK_ON { property = property }    
        //                   FILTER_ON { (propertyo = propertyo) }
        //                   TARGET { storage_type }";

        //    var tokens = new Lexer().Tokenize(input);

        //    var syntax = Expressions.Update.Parse(tokens);

        //    syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
        //        //fetch
        //        typeof(DeclareExpr),
        //        //properties
        //        typeof(PropertiesExpr),
        //        //data model
        //        typeof(DataModelExpr),
        //        //link
        //        typeof(LinkExpr),
        //        //filter
        //        typeof(FilterExpr),
        //        //target model
        //        typeof(TargetExpr)
        //        );
        //}

        //[Test]
        //public void Modify_MoreThanOneRowWithAggregate_ReturnSyntaxStructure()
        //{
        //    var input = @" MODIFY { Entity }
        //                   PROPERTIES { colum = '1', column = sum(column) }
        //                   DATA_MODEL { data, data}
        //                   LINK_ON { property = property }    
        //                   FILTER_ON { (propertyo = propertyo) }
        //                   TARGET { storage_type }";

        //    var tokens = new Lexer().Tokenize(input);

        //    var syntax = Expressions.Update.Parse(tokens);

        //    syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
        //        //fetch
        //        typeof(DeclareExpr),
        //        //properties
        //        typeof(PropertiesExpr),
        //        //data model
        //        typeof(DataModelExpr),
        //        //link
        //        typeof(LinkExpr),
        //        //filter
        //        typeof(FilterExpr),
        //        //target model
        //        typeof(TargetExpr)
        //        );
        //}

        //[Test]
        //public void Modify_MoreThanOneRowWithCondition_ReturnSyntaxStructure()
        //{
        //    var input = @" MODIFY { Entity }
        //                   PROPERTIES { colum = '1', column = sum(column) }
        //                   DATA_MODEL { data, data}
        //                   LINK_ON { property = property AND property = property OR property = property }    
        //                   FILTER_ON { (propertyo = propertyo) }
        //                   TARGET { storage_type }";

        //    var tokens = new Lexer().Tokenize(input);

        //    var syntax = Expressions.Update.Parse(tokens);

        //    syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
        //        //fetch
        //        typeof(DeclareExpr),
        //        //properties
        //        typeof(PropertiesExpr),
        //        //data model
        //        typeof(DataModelExpr),
        //        //link
        //        typeof(LinkExpr),
        //        //filter
        //        typeof(FilterExpr),
        //        //target model
        //        typeof(TargetExpr)
        //        );
        //}

        //[Test]
        //public void Modify_MoreThanOneRowWithRestriction_ReturnSyntaxStructure()
        //{
        //    var input = @" MODIFY { Entity }
        //                   PROPERTIES { colum = '1', column = sum(column) }
        //                   DATA_MODEL { data, data}
        //                   LINK_ON { property = property AND property = property OR property = property }    
        //                   FILTER_ON { (propertyo = propertyo) }
        //                   RESTRICT_TO { 1 }
        //                   TARGET { storage_type }";

        //    var tokens = new Lexer().Tokenize(input);

        //    var syntax = Expressions.Update.Parse(tokens);

        //    syntax.ParseTree.Select(x => x.GetType()).Should().Equal(
        //        //fetch
        //        typeof(DeclareExpr),
        //        //properties
        //        typeof(PropertiesExpr),
        //        //data model
        //        typeof(DataModelExpr),
        //        //link
        //        typeof(LinkExpr),
        //        //filter
        //        typeof(FilterExpr),
        //        //restrict
        //        typeof(RestrictExpr),
        //        //target model
        //        typeof(TargetExpr)
        //        );
        //}

    }
}
