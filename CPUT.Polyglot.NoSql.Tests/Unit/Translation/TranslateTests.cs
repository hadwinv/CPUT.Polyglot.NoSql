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
        public void Redis_FetchWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { }
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
        public void Redis_FetchWithKeyFilter_ReturnExecutableQuery()
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
        public void Redis_FetchWithoutKeyFilter_ReturnExecutableQuery()
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
        public void Redis_FetchWithMultipleFiltersIncludingKey_ReturnExecutableQuery()
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
        public void Redis_FetchWithMultipleFiltersExcludingKey_ReturnExecutableQuery()
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
        public void Cassandra_FetchWithoutFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchWithRestriction_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student LIMIT 10 ").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchOrderBy_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student ORDER BY firstname ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchOrderByDesc_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student ORDER BY firstname DESC").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchWithFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchWithMoreThanOneFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" AND idno = \"624083061345\"").Replace(" ", "")

            );
        }

        [Test]
        public void Cassandra_FetchWithMoreThanOneANDFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" AND idno = \"624083061345\" AND idno = \"624083061344\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchWithMoreThanOneOrFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" OR idno = \"624083061345\" OR idno = \"624083061344\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchWithORANDFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" AND idno = \"624083061345\" OR idno = \"624083061344\"").Replace(" ", "")
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
                ("UPDATE student SET firstname = \"Chuck T\" WHERE idno = \"62408306136\"").Replace(" ", "")
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
                ("UPDATE student SET firstname = \"Chuck T\", lastname = \"Tylers\" WHERE idno = \"62408306136\"").Replace(" ", "")
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
                ("UPDATE student SET firstname = \"Chuck T\"").Replace(" ", "")
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
                ("UPDATE student SET firstname = \"Chuck T\", lastname = \"Tylers\"").Replace(" ", "")
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
                ("INSERT INTO student (firstname) VALUES (\"Chuck T\")").Replace(" ", "")
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
                 ("INSERT INTO student (firstname, lastname) VALUES (\"Chuck T\", \"Tylers\")").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;
            
            var input = @"FETCH { student_no, nsum(marks) }
                    DATA_MODEL { transcript}
                    FILTER_ON {student_no = '05506604815'}
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
                ("SELECT idno, SUM(marks) as marks FROM grades WHERE idno = \"05506604815\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;
            
            var input = @"FETCH { student_no, navg(marks) }
                    DATA_MODEL { transcript}
                    FILTER_ON {student_no = '05506604815'}
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
                ("SELECT idno, AVG(marks) as marks FROM grades WHERE idno = \"05506604815\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchNCount_ReturnExecutableQuery()
        {
            List<Constructs> results = null;
            
            var input = @"FETCH { student_no, ncount(marks) }
                    DATA_MODEL { transcript}
                    FILTER_ON {student_no = '05506604815'}
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
                ("SELECT idno, COUNT(marks) as marks FROM grades WHERE idno = \"05506604815\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;
            
            var input = @"FETCH { student_no, nmin(marks) }
                    DATA_MODEL { transcript}
                    FILTER_ON {student_no = '05506604815'}
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
                ("SELECT idno, MIN(marks) as marks FROM grades WHERE idno = \"05506604815\"").Replace(" ", "")
            );
        }

        [Test]
        public void Cassandra_FetchNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;
            
            var input = @"FETCH { student_no, nmax(marks) }
                    DATA_MODEL { transcript}
                    FILTER_ON {student_no = '05506604815'}
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
                ("SELECT idno, MAX(marks) as marks FROM grades WHERE idno = \"05506604815\"").Replace(" ", "")
            );
        }

        #endregion

        #region MongoDB

        [Test]
        public void MongoDB_FetchWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
        public void MongoDB_FetchWithRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
        public void MongoDB_FetchOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
        public void MongoDB_FetchOrderByDesc_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
        public void MongoDB_FetchWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
        public void MongoDB_FetchWithMoreThanOneFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
                ("db.getCollection(\"students\").find({\"$and\" : [{\"id_number\" : \"62408306136\"},{\"id_number\" : \"624083061345\"}]},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", ""));
        }

        [Test]
        public void MongoDB_FetchWithMoreThanOneANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
                ("db.getCollection(\"students\").find({\"$and\" : [{\"id_number\" : \"62408306136\"},{\"id_number\" : \"624083061345\"},{\"id_number\" : \"624083061344\"}]},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchWithMoreThanOneOrFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
                ("db.getCollection(\"students\").find({\"$or\" : [{\"id_number\" : \"62408306136\"},{\"id_number\" : \"624083061345\"},{\"id_number\" : \"624083061344\"}]},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchWithORANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
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
                ("db.getCollection(\"students\").find({\"$and\" : [{\"id_number\" : \"62408306136\"},{\"id_number\" : \"624083061345\"}],\"$or\" : [{\"id_number\" : \"624083061344\"}]},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})").Replace(" ", "")
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

        [Test]
        public void MongoDB_AddMultipleEntries_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                          PROPERTIES {[{ name = 'Chuck T', surname = 'Tylers'}, { name = 'Taylor', surname = 'Test'}]}
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
                ("db.getCollection(\"students\").insertMany([{\"name\" : \"Chuck T\",  \"surname\" : \"Tylers\"}, {\"name\" : \"Taylor\",  \"surname\" : \"Test\"}])").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, nsum(t.subject.duration) }
                    DATA_MODEL { student AS s, transcript AS t }
                    LINK_ON { s.student_no = t.student_no }
                    FILTER_ON { s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND t.subject.duration > 0 }
                    GROUP_BY { s.idnumber, s.name, s.surname }
                    RESTRICT_TO { 2 }
                    ORDER_BY { s.name }
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
                ("db.getCollection(\"students\").aggregate(" +
                "[" +
                "   { $match : {" +
                "       \"$or\" : [" +
                "           {\"id_number\" : \"35808404617\"}," +
                "           {\"id_number\" : \"21708702176\"}" +
                "       ]," +
                "       \"$and\" : [" +
                "           {\"register.course.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$register.course.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$register.course.subjects\"}}," +
                "   { $group : { " +
                "       _id: \"$_id\", " +
                "       name : { \"$first\" : \"$name\"}, " +
                "       surname : { \"$first\" : \"$surname\"}, " +
                "       id_number : { \"$first\" : \"$id_number\"}, " +
                "       duration: { $sum: \"$subjects.duration\"}}}, " +
                "   { $sort : { name : 1 } }, " +
                "   { $limit : 2 }])").Replace(" ", "")
            );
        }
    
        [Test]
    
        public void MongoDB_FetchNCOUNT_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, ncount(t.subject.duration) }
                    DATA_MODEL { student AS s, transcript AS t }
                    LINK_ON { s.student_no = t.student_no }
                    FILTER_ON { s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND t.subject.duration > 0 }
                    GROUP_BY { s.idnumber, s.name, s.surname }
                    RESTRICT_TO { 2 }
                    ORDER_BY { s.name }
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
                ("db.getCollection(\"students\").aggregate(" +
                "[" +
                "   { $match : {" +
                "       \"$or\" : [" +
                "           {\"id_number\" : \"35808404617\"}," +
                "           {\"id_number\" : \"21708702176\"}" +
                "       ]," +
                "       \"$and\" : [" +
                "           {\"register.course.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$register.course.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$register.course.subjects\"}}," +
                "   { $group : { " +
                "       _id: \"$_id\", " +
                "       name : { \"$first\" : \"$name\"}, " +
                "       surname : { \"$first\" : \"$surname\"}, " +
                "       id_number : { \"$first\" : \"$id_number\"}, " +
                "       duration: { $count: {}}}}, " +
                "   { $sort : { name : 1 } }, " +
                "   { $limit : 2 }])").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, navg(t.subject.duration) }
                    DATA_MODEL { student AS s, transcript AS t }
                    LINK_ON { s.student_no = t.student_no }
                    FILTER_ON { s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND t.subject.duration > 0 }
                    GROUP_BY { s.idnumber, s.name, s.surname }
                    RESTRICT_TO { 2 }
                    ORDER_BY { s.name }
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
                ("db.getCollection(\"students\").aggregate(" +
                "[" +
                "   { $match : {" +
                "       \"$or\" : [" +
                "           {\"id_number\" : \"35808404617\"}," +
                "           {\"id_number\" : \"21708702176\"}" +
                "       ]," +
                "       \"$and\" : [" +
                "           {\"register.course.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$register.course.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$register.course.subjects\"}}," +
                "   { $group : { " +
                "       _id: \"$_id\", " +
                "       name : { \"$first\" : \"$name\"}, " +
                "       surname : { \"$first\" : \"$surname\"}, " +
                "       id_number : { \"$first\" : \"$id_number\"}, " +
                "       duration: { $avg: \"$subjects.duration\"}}}, " +
                "   { $sort : { name : 1 } }, " +
                "   { $limit : 2 }])").Replace(" ", "")
        );
        }

        [Test]
        public void MongoDB_FetchNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, nmin(t.subject.duration) }
                    DATA_MODEL { student AS s, transcript AS t }
                    LINK_ON { s.student_no = t.student_no }
                    FILTER_ON { s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND t.subject.duration > 0 }
                    GROUP_BY { s.idnumber, s.name, s.surname }
                    RESTRICT_TO { 2 }
                    ORDER_BY { s.name }
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
                ("db.getCollection(\"students\").aggregate(" +
                "[" +
                "   { $match : {" +
                "       \"$or\" : [" +
                "           {\"id_number\" : \"35808404617\"}," +
                "           {\"id_number\" : \"21708702176\"}" +
                "       ]," +
                "       \"$and\" : [" +
                "           {\"register.course.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$register.course.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$register.course.subjects\"}}," +
                "   { $group : { " +
                "       _id: \"$_id\", " +
                "       name : { \"$first\" : \"$name\"}, " +
                "       surname : { \"$first\" : \"$surname\"}, " +
                "       id_number : { \"$first\" : \"$id_number\"}, " +
                "       duration: { $min: \"$subjects.duration\"}}}, " +
                "   { $sort : { name : 1 } }, " +
                "   { $limit : 2 }])").Replace(" ", "")
            );
        }

        [Test]
        public void MongoDB_FetchNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, nmax(t.subject.duration) }
                    DATA_MODEL { student AS s, transcript AS t }
                    LINK_ON { s.student_no = t.student_no }
                    FILTER_ON { s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND t.subject.duration > 0 }
                    GROUP_BY { s.idnumber, s.name, s.surname }
                    RESTRICT_TO { 2 }
                    ORDER_BY { s.name }
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
                ("db.getCollection(\"students\").aggregate(" +
                "[" +
                "   { $match : {" +
                "       \"$or\" : [" +
                "           {\"id_number\" : \"35808404617\"}," +
                "           {\"id_number\" : \"21708702176\"}" +
                "       ]," +
                "       \"$and\" : [" +
                "           {\"register.course.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$register.course.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$register.course.subjects\"}}," +
                "   { $group : { " +
                "       _id: \"$_id\", " +
                "       name : { \"$first\" : \"$name\"}, " +
                "       surname : { \"$first\" : \"$surname\"}, " +
                "       id_number : { \"$first\" : \"$id_number\"}, " +
                "       duration: { $max: \"$subjects.duration\"}}}, " +
                "   { $sort : { name : 1 } }, " +
                "   { $limit : 2 }])").Replace(" ", "")
        );
        }

        #endregion

        #region Neo4j

        [Test]
        public void Neo4j_FetchWithoutFilter_ReturnExecutableQuery()
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
        public void Neo4j_FetchWithRestriction_ReturnExecutableQuery()
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
        public void Neo4j_FetchOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { title, name, surname, idnumber, dateofbirth }
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
                ("MATCH ( pup:Pupil ) RETURN pup.title, pup.name, pup.surname, pup.idnumber, pup.dob ORDER BY pup.name ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchOrderByDesc_ReturnExecutableQuery()
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
        public void Neo4j_FetchWithFilter_ReturnExecutableQuery()
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
        public void Neo4j_FetchWithMoreThanOneFilter_ReturnExecutableQuery()
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
        public void Neo4j_FetchWithMoreThanOneANDFilter_ReturnExecutableQuery()
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
        public void Neo4j_FetchWithMoreThanOneOrFilter_ReturnExecutableQuery()
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
        public void Neo4j_FetchWithORANDFilter_ReturnExecutableQuery()
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
        public void Neo4j_FetchUnwinded_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, marks }
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
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as mar RETURN mar.Grade, mar.Score").Replace(" ", "")
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
        public void Neo4j_FetchSingleRelation_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, t.gradedsymbol, nmax(t.marks) }
                        DATA_MODEL { student AS s, transcript AS t}
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
                ("MATCH (s:Pupil)-[:TRANSCRIPT]->(t:Progress) UNWIND apoc.convert.fromJsonList( t.marks) as mar RETURN s.title, s.name, s.surname, mar.Grade,  MAX(mar.Score) as Score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchMoreThanOneRelation_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, t.gradedsymbol, c.name, nmax(t.marks) }
                        DATA_MODEL { student AS s, transcript AS t, course AS c}
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
                ("MATCH (t:Progress)<-[:TRANSCRIPT]-(s:Pupil)-[:ENROLLED_IN]->(c:Course) UNWIND apoc.convert.fromJsonList( t.marks) as mar RETURN s.title, s.name, s.surname, mar.Grade, c.name,  MAX(mar.Score) as Score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchMoreThanThreeRelation_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, t.gradedsymbol, c.name, su.name, nmax(t.marks) }
                        DATA_MODEL { student AS s, transcript AS t, course AS c, subject AS su}
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
                ("MATCH (t:Progress)<-[:TRANSCRIPT]-(s:Pupil)-[:ENROLLED_IN]->(c:Course)OPTIONAL MATCH (c)-[:CONTAINS]->(su:Subject) UNWIND apoc.convert.fromJsonList( t.marks) as mar RETURN s.title, s.name, s.surname, mar.Grade, c.name, su.name,  MAX(mar.Score) as Score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, nsum(marks) }
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
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as mar RETURN mar.Grade, SUM(mar.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, navg(marks) }
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
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as mar RETURN mar.Grade, AVG(mar.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedNCount_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, ncount(marks) }
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
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as mar RETURN mar.Grade, COUNT(mar.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, nmin(marks) }
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
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as mar RETURN mar.Grade, MIN(mar.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchUnwindedNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { gradedsymbol, nmax(marks) }
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
                ("MATCH ( pro:Progress ) UNWIND apoc.convert.fromJsonList(pro.marks) as mar RETURN mar.Grade, MAX(mar.Score) as Score").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, nsum(cost) }
                            DATA_MODEL { subject}
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
                ("MATCH ( sub:Subject ) RETURN sub.name, SUM(sub.cost) as cost").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, navg(cost) }
                            DATA_MODEL { subject}
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
                ("MATCH ( sub:Subject ) RETURN sub.name, AVG(sub.cost) as cost").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchNCount_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, ncount(cost) }
                            DATA_MODEL { subject}
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
                ("MATCH ( sub:Subject ) RETURN sub.name, COUNT(sub.cost) as cost").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, nmin(cost) }
                            DATA_MODEL { subject}
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
                ("MATCH ( sub:Subject ) RETURN sub.name, MIN(sub.cost) as cost").Replace(" ", "")
            );
        }

        [Test]
        public void Neo4j_FetchNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, nmax(cost) }
                            DATA_MODEL { subject}
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
                ("MATCH ( sub:Subject ) RETURN sub.name, MAX(sub.cost) as cost").Replace(" ", "")
            );
        }
    

        #endregion

    }
}
