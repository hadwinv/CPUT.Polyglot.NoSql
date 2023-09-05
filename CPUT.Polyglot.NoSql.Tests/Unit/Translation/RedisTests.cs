using App.Metrics;
using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Interface;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Mapper;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using CPUT.Polyglot.NoSql.Translator;
using CPUT.Polyglot.NoSql.Translator.Events;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Superpower;
using System.Collections.Generic;
using System.Linq;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Translation
{
    [TestFixture]
    public class RedisTests
    {
        private Mock<ICache> _mockCache;
        private Mock<IMetrics> _mockMetrics;

        private ITranslate _translate;
        private IInterpreter _interpreter;
        private ISchema _schema;

        [SetUp]
        public void SetUp()
        {
            _mockCache = new Mock<ICache>();

            _interpreter = new Interpreter();
            _schema = new Schema(_mockCache.Object);

            _mockMetrics = new Mock<IMetrics>();

            _translate = new Translate(_interpreter, _schema, _mockMetrics.Object);
        }

        [Test]
        public void Convert01_FetchWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student }
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.FETCH.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query.Replace(" ", "")).Should().Equal(
                ("KEYS *").Replace(" ", "")
            );
        }

        [Test]
        public void Convert02_FetchWithKeyFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136'}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.FETCH.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query.Replace(" ", "")).Should().Equal(
                ("GET \"62408306136\"").Replace(" ", "")
            );
        }

        [Test]
        public void Convert03_FetchWithoutKeyFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {name = 'test'}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.FETCH.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query.Replace(" ", "")).Should().Equal(
                ("KEYS *").Replace(" ", "")
            );

        }

        [Test]
        public void Convert04_FetchWithMultipleFiltersIncludingKey_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306148' AND name = 'Test' AND surname = 'Test'}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.FETCH.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query.Replace(" ", "")).Should().Equal(
                ("GET \"62408306148\"").Replace(" ", "")
            );
        }

        [Test]
        public void Convert05_FetchWithMultipleFiltersExcludingKey_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {name = 'Test' AND surname = 'Test'}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.FETCH.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query.Replace(" ", "")).Should().Equal(
                ("KEYS *").Replace(" ", "")
            );
        }

        [Test]
        public void Convert06_ModifyPropertyWithKeyFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            FILTER_ON { idnumber = '62408306136' }
                            TARGET { redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.MODIFY.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query.Replace(" ", "")).Should().Equal(
                ("GET \"62408306136\";SET 62408306136 {0}").Replace(" ", "")
            );
        }

        [Test]
        public void Convert07_ModifyPropertyWithoutKeyFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            FILTER_ON { name = 'Chuck' }
                            TARGET { redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.MODIFY.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query.Replace(" ", "")).Should().Equal(
                ("KEYS * ;SET {0} {0}").Replace(" ", "")
            );
        }

        [Test]
        public void Convert08_AddPropertyWithKey_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                            PROPERTIES { idnumber = '624083061364', name = 'Chuck T', surname = 'Tester'}
                            TARGET { redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.ADD.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query.Replace(" ", "")).Should().Equal(
                ("SET 624083061364 {0}").Replace(" ", "")
            );
        }

        [Test]
        public void Convert09_AddPropertyWithoutKey_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                            PROPERTIES { name = 'Chuck T', surname = 'Tester'}
                            TARGET { redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.ADD.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Result.Query).Should().Equal(
                ("Cannot generate Redis command")
            );
        }
    }
}
