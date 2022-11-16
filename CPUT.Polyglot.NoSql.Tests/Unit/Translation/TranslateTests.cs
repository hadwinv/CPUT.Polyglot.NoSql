using Cassandra;
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
using CPUT.Polyglot.NoSql.Translator.Producers.Parts;
using FluentAssertions;
using Moq;
using Neo4jClient.Cypher;
using NUnit.Framework;
using StackExchange.Redis;
using Superpower;
using System;
using System.Collections.Generic;
using System.Linq;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static System.Formats.Asn1.AsnWriter;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Translation
{
    [TestFixture]
    public class TranslateTests
    {
        private Mock<ICache> _mockCache;

        private ITranslate _translate;
        private IInterpreter _interpreter;
        private ISchema _schema;

        [SetUp]
        public void SetUp()
        {
            _mockCache = new Mock<ICache>();

            _interpreter = new Interpreter();
            _schema = new Schema(_mockCache.Object);

            _translate = new Translate(_interpreter, _schema);
        }

        #region Redis

        [Test]
        public void Redis_FetchStudentWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("KEYS *").Replace(" ", "")
            );
        }

        [Test]
        public void Redis_FetchStudentWithKeyFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136'}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("GET \"62408306136\"").Replace(" ", "")
            );
        }

        [Test]
        public void Redis_FetchStudentWithoutKeyFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {name = 'test'}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("KEYS *").Replace(" ", "")
            );

        }

        [Test]
        public void Redis_FetchStudentWithMultipleFiltersIncludingKey_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306148' AND name = 'Test' AND surname = 'Test'}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("GET \"62408306148\"").Replace(" ", "")
            );
        }

        [Test]
        public void Redis_FetchStudentWithMultipleFiltersExcludingKey_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {name = 'Test' AND surname = 'Test'}
                    TARGET {  redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("KEYS *").Replace(" ", "")
            );
        }

        [Test]
        public void Redis_ModifyPropertyWithKeyFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            FILTER_ON { idnumber = '62408306136' }
                            TARGET { redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("GET \"62408306136\";SET 62408306136 {0}").Replace(" ", "")
            );
        }

        [Test]
        public void Redis_ModifyPropertyWithoutKeyFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            FILTER_ON { name = 'Chuck' }
                            TARGET { redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("KEYS * ;SET {0} {0}").Replace(" ", "")
            );
        }


        [Test]
        public void Redis_AddPropertyWithKey_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                            PROPERTIES { idnumber = '624083061364', name = 'Chuck T', surname = 'Tester'}
                            TARGET { redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Insert.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SET 624083061364 {0}").Replace(" ", "")
            );
        }

        [Test]
        public void Redis_AddPropertyWithoutKey_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                            PROPERTIES { name = 'Chuck T', surname = 'Tester'}
                            TARGET { redis }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Insert.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Query).Should().Equal(
                ("Cannot generate Redis command")
            );
        }

        #endregion

        #region Cassandra

        [Test]
        public void Cassandra_FetchStudentWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchStudentWithRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    RESTRICT_TO { 10 }
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students LIMIT 10 ").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchStudentOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    ORDER_BY { name} 
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students ORDER BY firstname ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchStudentOrderByDesc_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    ORDER_BY { name DESC} 
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students ORDER BY firstname DESC").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchStudentWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136'}
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students WHERE idno = \"62408306136\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchStudentWithMoreThanOneFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345'}
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students WHERE idno = \"62408306136\" AND idno = \"624083061345\"").Replace(" ", "")

            );
        }

        [Test]
        public void Cassandra_FetchStudentWithMoreThanOneANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345' AND idnumber = '624083061344'}
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students WHERE idno = \"62408306136\" AND idno = \"624083061345\" AND idno = \"624083061344\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchStudentWithMoreThanOneOrFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' OR idnumber = '624083061345' OR idnumber = '624083061344'}
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students WHERE idno = \"62408306136\" OR idno = \"624083061345\" OR idno = \"624083061344\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchStudentWithORANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345' OR idnumber = '624083061344'}
                    TARGET {  cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("SELECT firstname, lastname, idno, dob FROM students WHERE idno = \"62408306136\" AND idno = \"624083061345\" OR idno = \"624083061344\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_ModifySinglePropertyWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            FILTER_ON { idnumber = '62408306136' }
                            TARGET { cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("UPDATE students SET firstname = \"Chuck T\" WHERE idno = \"62408306136\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_ModifyMultiplePropertiesWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                            FILTER_ON { idnumber = '62408306136' }
                            TARGET { cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("UPDATE students SET firstname = \"Chuck T\", lastname = \"Tylers\" WHERE idno = \"62408306136\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_ModifySinglePropertyWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            TARGET { cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("UPDATE students SET firstname = \"Chuck T\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_ModifyMultiplePropertiesWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                            TARGET { cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("UPDATE students SET firstname = \"Chuck T\", lastname = \"Tylers\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_AddSingleProperty_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                          PROPERTIES { name = 'Chuck T'}
                          TARGET { cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Insert.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("INSERT INTO students (firstname) VALUES (\"Chuck T\")").Replace(" ", "")
            );
        }


        [Test]
        public void Cassandra_AddMultiplePropertieWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                          PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                          TARGET { cassandra }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Insert.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                 ("INSERT INTO students (firstname, lastname) VALUES (\"Chuck T\", \"Tylers\")").Replace(" ", "")
            );
        }

        #endregion

        #region MongoDB

        [Test]
        public void MongoDB_FetchStudentWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
            );

        }

        [Test]
        public void MongoDB_FetchStudentWithRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    RESTRICT_TO { 10 }
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({},{name : 1, surname : 1, id_number : 1, date_of_birth : 1}).limit(10)").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchStudentOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    ORDER_BY { name} 
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})._addSpecial(\"$orderby\", { name : 1 })").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchStudentOrderByDesc_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    ORDER_BY { name DESC} 
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})._addSpecial(\"$orderby\", { name : -1 })").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchStudentWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136'}
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({\"id_number\" : \"62408306136\"},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
            );

        }

        [Test]
        public void MongoDB_FetchStudentWithMoreThanOneFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345'}
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({\"$and\" : [{\"id_number\" : \"624083061345\"}, {\"id_number\" : \"62408306136\"}]},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
                
            );
        }

        [Test]
        public void MongoDB_FetchStudentWithMoreThanOneANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345' AND idnumber = '624083061344'}
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({\"$and\" : [{\"id_number\" : \"624083061345\"},{\"id_number\" : \"624083061344\"},{\"id_number\" : \"62408306136\"}]},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchStudentWithMoreThanOneOrFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' OR idnumber = '624083061345' OR idnumber = '624083061344'}
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({\"$or\" : [{\"id_number\" : \"624083061345\"},{\"id_number\" : \"624083061344\"},{\"id_number\" : \"62408306136\"}]},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchStudentWithORANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345' OR idnumber = '624083061344'}
                    TARGET {  mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").find({\"$or\" : [{\"id_number\" : \"624083061344\"}],\"$and\" : [{\"id_number\" : \"624083061345\"},{\"id_number\" : \"62408306136\"}]},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_ModifySinglePropertyWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            FILTER_ON { idnumber = '62408306136' }
                            TARGET { mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").updateMany({\"id_number\" : \"62408306136\"},{$set: {\"name\" : \"Chuck T\"}})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_ModifyMultiplePropertiesWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                            FILTER_ON { idnumber = '62408306136' }
                            TARGET { mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").updateMany({\"id_number\" : \"62408306136\"},{$set: {\"name\" : \"Chuck T\", \"surname\" : \"Tylers\"}})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_ModifySinglePropertyWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            TARGET { mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").updateMany({},{$set: {\"name\" : \"Chuck T\"}})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_ModifyMultiplePropertiesWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                            TARGET { mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").updateMany({},{$set: {\"name\" : \"Chuck T\", \"surname\" : \"Tylers\"}})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_AddSinglePropertyWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                          PROPERTIES { name = 'Chuck T'}
                          TARGET { mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Insert.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").insertMany([{\"name\" : \"Chuck T\"}])").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_AddMultiplePropertieWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                          PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                          TARGET { mongodb }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Insert.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("db.getCollection(\"students\").insertMany([{\"name\" : \"Chuck T\",  \"surname\" : \"Tylers\"}])").Replace(" ", "")
            );
        }

        #endregion

        #region Neo4j

        [Test]
        public void Neo4j_FetchStudentWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil ) RETURN pup.name, pup.surname, pup.idnumber, pup.dob").Replace(" ", "")
            );
        }


        [Test]
        public void Neo4j_FetchStudentWithRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    RESTRICT_TO { 10 }
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil ) RETURN pup.name, pup.surname, pup.idnumber, pup.dob LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchStudentOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    ORDER_BY { name} 
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil ) RETURN pup.name, pup.surname, pup.idnumber, pup.dob ORDER BY pup.name ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchStudentOrderByDesc_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    ORDER_BY { name DESC} 
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil ) RETURN pup.name, pup.surname, pup.idnumber, pup.dob ORDER BY pup.name DESC").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchStudentWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136'}
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil )  WHERE pup.idnumber = \"62408306136\" RETURN pup.name, pup.surname, pup.idnumber, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchStudentWithMoreThanOneFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345'}
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil )  WHERE pup.idnumber = \"62408306136\" AND pup.idnumber = \"624083061345\" RETURN pup.name, pup.surname, pup.idnumber, pup.dob").Replace(" ", "")

            );
        }

        [Test]
        public void Neo4j_FetchStudentWithMoreThanOneANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345' AND idnumber = '624083061344'}
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil )  WHERE pup.idnumber = \"62408306136\" AND pup.idnumber = \"624083061345\" AND pup.idnumber = \"624083061344\" RETURN pup.name, pup.surname, pup.idnumber, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchStudentWithMoreThanOneOrFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' OR idnumber = '624083061345' OR idnumber = '624083061344'}
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil )  WHERE pup.idnumber = \"62408306136\" OR pup.idnumber = \"624083061345\" OR pup.idnumber = \"624083061344\" RETURN pup.name, pup.surname, pup.idnumber, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchStudentWithORANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345' OR idnumber = '624083061344'}
                    TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil )  WHERE pup.idnumber = \"62408306136\" AND pup.idnumber = \"624083061345\" OR pup.idnumber = \"624083061344\" RETURN pup.name, pup.surname, pup.idnumber, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_ModifySinglePropertyWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            FILTER_ON { idnumber = '62408306136' }
                            TARGET { neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil ) WHERE pup.idnumber = \"62408306136\" SET pup.name = \"Chuck T\"").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_ModifyMultiplePropertiesWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                            FILTER_ON { idnumber = '62408306136' }
                            TARGET { neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil ) WHERE pup.idnumber = \"62408306136\" SET pup.name = \"Chuck T\", pup.surname = \"Tylers\"").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_ModifySinglePropertyWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T'}
                            TARGET { neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil ) SET pup.name = \"Chuck T\"").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_ModifyMultiplePropertiesWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                            PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                            TARGET { neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Update.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.MODIFY
                           });

            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pup:Pupil ) SET pup.name = \"Chuck T\", pup.surname = \"Tylers\"").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_AddSingleProperty_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                          PROPERTIES { name = 'Chuck T'}
                          TARGET { neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Insert.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("CREATE ( pup:Pupil { name : \"Chuck T\"} )").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_AddMultiplePropertieWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                          PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                          TARGET { neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Insert.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.ADD
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                 ("CREATE ( pup:Pupil { name : \"Chuck T\", surname : \"Tylers\"} )").Replace(" ", "")
            );
        }

        [Test]        
        public void Neo4j_FetchUnwindedSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, sum(marks) }
                            DATA_MODEL { transcript}
                            TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as ma RETURN ma.Grade, SUM(ma.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, avg(marks) }
                            DATA_MODEL { transcript}
                            TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as ma RETURN ma.Grade, AVG(ma.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedCount_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, Count(marks) }
                            DATA_MODEL { transcript}
                            TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as ma RETURN ma.Grade, COUNT(ma.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, min(marks) }
                            DATA_MODEL { transcript}
                            TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as ma RETURN ma.Grade, MIN(ma.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, max(marks) }
                            DATA_MODEL { transcript}
                            TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as ma RETURN ma.Grade, MAX(ma.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchSingleRelation_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { title, name, surname, gradedsymbol, max(marks) }
                            DATA_MODEL { student, transcript}
                            RESTRICT_TO { 10 }
                            TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH (pup:Pupil)-[:TRANSCRIPT]->(pro:Progress) UNWIND apoc.convert.fromJsonList( pro.marks) as ma RETURN pup.title, pup.name, pup.surname, ma.Grade,  MAX(ma.Score) as Score LIMIT 10").Replace(" ", "")
            );
        }


        [Test]
        public void Neo4j_FetchMoreThanOneRelation_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { title, name, surname, gradedsymbol, name, max(marks) }
                            DATA_MODEL { student, transcript, course}
                            RESTRICT_TO { 10 }
                            TARGET {  neo4j }";

            var tokens = new Lexer().Tokenize(input);

            //generate abstract syntax tree
            var syntaxExpr = Expressions.Select.Parse(tokens);

            var transformed = _translate.Convert(
                           new ConstructPayload
                           {
                               BaseExpr = syntaxExpr,
                               Command = Utils.Command.FETCH
                           });


            results = transformed.Result;

            results.Select(x => x.Query.Replace(" ", "")).Should().Equal(
                ("MATCH (cou:Course)<-[:ENROLLED_IN]-(pup:Pupil)-[:TRANSCRIPT]->(pro:Progress) UNWIND apoc.convert.fromJsonList( pro.marks) as ma RETURN pup.title, pup.name, pup.surname, ma.Grade, cou.name,  MAX(ma.Score) as Score LIMIT 10").Replace(" ", "")
            );
        }

        #endregion

    }
}
