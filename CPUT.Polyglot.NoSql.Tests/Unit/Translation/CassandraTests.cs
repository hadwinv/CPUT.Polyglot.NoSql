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
    public class CassandraTests
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

        [Test]
        public void Convert01_SimpleFetchWithoutFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert02_SimpleFetchWithRestriction_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student LIMIT 10;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert03_SimpleFetchOrderBy_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student ORDER BY firstname ASC;").Replace(" ", "")
            );
            // SELECT firstname, lastname, idno, dob FROM student ORDER BY firstname  ASC ;
        }

        [Test]
        public void Convert04_SimpleFetchOrderByMoreThanOne_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    ORDER_BY { name, surname} 
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
                ("SELECT firstname, lastname, idno, dob FROM student ORDER BY firstname ASC, lastname ASC;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert05_SimpleFetchOrderByDesc_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student ORDER BY firstname DESC;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert06_SimpleFetchWithFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert07_SimpleFetchWithMoreThanOneFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" AND idno = \"624083061345\";").Replace(" ", "")

            );
        }

        [Test]
        public void Convert08_SimpleFetchWithMoreThanOneANDFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" AND idno = \"624083061345\" AND idno = \"624083061344\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert09_SimpleFetchWithMoreThanOneOrFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" OR idno = \"624083061345\" OR idno = \"624083061344\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert10_SimpleFetchWithORANDFilter_ReturnExecutableQuery()
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" AND idno = \"624083061345\" OR idno = \"624083061344\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert11_ComplexFetchOneToOneObject_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.subject, s.transcript.result }
                    DATA_MODEL { student AS s}
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
                ("SELECT grades.subject,  grades.marks FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert12_ComplexFetchOneToOneObjectWithPropertName_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.subject.name, s.transcript.result }
                    DATA_MODEL { student AS s}
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
                ("SELECT grades.subject.descr,  grades.marks FROM student;").Replace(" ", "")
            );
        }

        public void Convert13_ComplexFetchOneToManyObject_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.register.subjects }
                    DATA_MODEL { student AS s}
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
                ("SELECT registered.subject FROM student;").Replace(" ", "")
            );
        }

        #region Aggregation

        [Test]
        public void Convert14_ComplexFetchNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { t.identifier, nsum(t.transcript.result)}
                    DATA_MODEL { student AS t}
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
                ("SELECT studentno, SUM(grades.marks) as marks FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert15_ComplexFetchNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { t.identifier, navg(t.transcript.result)}
                    DATA_MODEL { student AS t}
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
                ("SELECT studentno, AVG(grades.marks) as marks FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert16_ComplexFetchNCount_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { t.identifier, ncount(t.transcript.result)}
                    DATA_MODEL { student AS t}
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
                ("SELECT studentno, COUNT(grades.marks) as marks FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert17_ComplexFetchNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { t.identifier, nmin(t.transcript.result)}
                    DATA_MODEL { student AS t}
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
                ("SELECT studentno, MIN(grades.marks) as marks FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert18_ComplexFetchNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { t.identifier, nmax(t.transcript.result)}
                    DATA_MODEL { student AS t}
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
                ("SELECT studentno, MAX(grades.marks) as marks FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert19_ComplexFetchWithFilterNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { t.identifier, nsum(t.transcript.result)}
                    DATA_MODEL { student AS t}
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
                ("SELECT studentno, SUM(grades.marks) as marks FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert20_ComplexFetchWithFilterNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  t.identifier, navg(t.transcript.result) }
                    DATA_MODEL { student AS t}
                    FILTER_ON { t.idnumber = '05506604815'}
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
                ("SELECT studentno, AVG(grades.marks) as marks FROM student WHERE idno = \"05506604815\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert21_ComplexFetchWithFilterNCount_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  t.identifier, ncount(t.transcript.result) }
                    DATA_MODEL { student AS t}
                    FILTER_ON { t.idnumber = '05506604815'}
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
                 ("SELECT studentno, COUNT(grades.marks) as marks FROM student WHERE idno = \"05506604815\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert22_ComplexFetchWithFilterNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  t.identifier, nmin(t.transcript.result) }
                    DATA_MODEL { student AS t}
                    FILTER_ON { t.idnumber = '05506604815'}
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
                ("SELECT studentno, MIN(grades.marks) as marks FROM student WHERE idno = \"05506604815\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert23_ComplexFetchWithFilterNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  t.identifier, nmax(t.transcript.result) }
                    DATA_MODEL { student AS t}
                    FILTER_ON { t.idnumber = '05506604815'}
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
                ("SELECT studentno, MAX(grades.marks) as marks FROM student WHERE idno = \"05506604815\";").Replace(" ", "")
            );
        }

        #endregion

        [Test]
        public void Convert24_ModifySinglePropertyWithSingleFilter_ReturnExecutableQuery()
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
                ("UPDATE student SET firstname = \"Chuck T\" WHERE idno = \"62408306136\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert25_ModifyMultiplePropertiesWithSingleFilter_ReturnExecutableQuery()
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
                ("UPDATE student SET firstname = \"Chuck T\", lastname = \"Tylers\" WHERE idno = \"62408306136\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert26_ModifySinglePropertyWithoutFilter_ReturnExecutableQuery()
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
                ("UPDATE student SET firstname = \"Chuck T\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert27_ModifyMultiplePropertiesWithoutFilter_ReturnExecutableQuery()
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
                ("UPDATE student SET firstname = \"Chuck T\", lastname = \"Tylers\";").Replace(" ", "")
            );
        }

        [Test]
        public void Convert28_AddSingleProperty_ReturnExecutableQuery()
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
                ("INSERT INTO student (firstname) VALUES (\"Chuck T\");").Replace(" ", "")
            );
        }

        [Test]
        public void Convert29_AddMultiplePropertieWithoutFilter_ReturnExecutableQuery()
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
                 ("INSERT INTO student (firstname, lastname) VALUES (\"Chuck T\", \"Tylers\");").Replace(" ", "")
            );
        }

        [Test]
        public void Convert30_FetchJsonWithoutAlias_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { transcript.subject, transcript.result }
                    DATA_MODEL { student }
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
                ("SELECT grades.subject,  grades.marks FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert31_SimpleFetchWithNonIndexedFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {initial = 'test'}
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
                ("SELECT firstname, lastname, idno, dob FROM student WHERE initials = \"test\" ALLOW FILTERING;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert32_SimpleFetchAll_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { identifier, idnumber, title, preferredname, initial, name, surname, dateofbirth, gender,
				                  address, contact, register, transcript }
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
                ("SELECT studentno, idno, title, aka, initials, firstname, lastname, dob, genderid, address, registered, grades FROM student;").Replace(" ", "")
            );
        }

        [Test]
        public void Convert33_ComplexFetchAll_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { identifier, idnumber, title, preferredname, initial, name, surname, dateofbirth, gender,
				                  address, contact, register, transcript }
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
                ("SELECT studentno, idno, title, aka, initials, firstname, lastname, dob, genderid, address, registered, grades FROM student;").Replace(" ", "")
            );
        }
    }
}
