using Cassandra.Mapping;
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
using Neo4jClient.Cypher;
using NUnit.Framework;
using Superpower;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Translation
{
    [TestFixture]
    public class Neo4jTests
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
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil ) RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Convert02_SimpleFetchWithRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                RESTRICT_TO { 10 }
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil ) RETURN pup.name, pup.surname, pup.idnum, pup.dob LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert03_SimpleFetchOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { title, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                ORDER_BY { name} 
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil ) RETURN pup.title, pup.name, pup.surname, pup.idnum, pup.dob ORDER BY pup.name ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Convert04_SimpleFetchMoreThanOneOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { title, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                ORDER_BY { name, surname} 
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil ) RETURN pup.title, pup.name, pup.surname, pup.idnum, pup.dob ORDER BY pup.name ASC, pup.surname ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Convert05_SimpleFetchOrderByDesc_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                ORDER_BY { name DESC} 
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil ) RETURN pup.name, pup.surname, pup.idnum, pup.dob ORDER BY pup.name DESC").Replace(" ", "")
            );
        }

        [Test]
        public void Convert06_SimpleFetchWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                FILTER_ON {idnumber = '62408306136'}
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil )  WHERE pup.idnum = \"62408306136\" RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Convert07_SimpleFetchWithMoreThanOneFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345'}
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil )  WHERE pup.idnum = \"62408306136\" AND pup.idnum = \"624083061345\" RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", "")

            );
        }

        [Test]
        public void Convert08_SimpleFetchWithMoreThanOneANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345' AND idnumber = '624083061344'}
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil )  WHERE pup.idnum = \"62408306136\" AND pup.idnum = \"624083061345\" AND pup.idnum = \"624083061344\" RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Convert09_SimpleFetchWithMoreThanOneOrFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                FILTER_ON {idnumber = '62408306136' OR idnumber = '624083061345' OR idnumber = '624083061344'}
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil )  WHERE pup.idnum = \"62408306136\" OR pup.idnum = \"624083061345\" OR pup.idnum = \"624083061344\" RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Convert10_SimpleFetchWithORANDFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                DATA_MODEL { student}
                FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345' OR idnumber = '624083061344'}
                TARGET {  neo4j }";

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
                ("MATCH ( pup:pupil )  WHERE pup.idnum = \"62408306136\" AND pup.idnum = \"624083061345\" OR pup.idnum = \"624083061344\" RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", "")
            );
        }

        [Test]
        public void Convert11_ComplexFetchUnwinded_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.symbol, s.transcript.result }
                        DATA_MODEL { student AS s}
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";

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
                ("MATCH (pro:progress ) UNWIND apoc.convert.fromJsonList(pro.results) as res RETURN res.grade, res.score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert12_ComplexFetchUnwindedWithFilters_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.symbol, s.transcript.result }
                        DATA_MODEL { student AS s}
                        FILTER_ON {s.transcript.result > 0 AND s.transcript.symbol = 'A' }
                        TARGET {  neo4j }";



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
                ("MATCH (pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res WITH res WHERE res.score > 0 AND res.grade = \"A\" RETURN res.grade, res.score").Replace(" ", "")
            );
        }

        [Test]
        public void Convert13_ComplexFetchSingleRelation_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, s.transcript.symbol, s.transcript.result }
                        DATA_MODEL { student AS s}
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";

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
                ("MATCH (pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res RETURN pup.name, pup.surname,  pup.idnum, res.grade,  res.score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert14_ComplexFetchMoreThanOneRelation_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, s.register.course.name, s.transcript.result }
                        DATA_MODEL { student AS s}
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";

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
                ("MATCH (cou:course)<-[:ENROLLED_IN]-(pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res RETURN pup.title, pup.name, pup.surname, cou.description, res.score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert15_ComplexFetchMoreThanThreeRelation_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, s.register.course.name,  s.register.subject.name, s.transcript.result }
                        DATA_MODEL { student AS s}
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";

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
                ("MATCH (sub:subject)<-[:CONTAINS]-(cou:course)<-[:ENROLLED_IN]-(pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res RETURN pup.title, pup.name, pup.surname, cou.description, sub.description, res.score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert16_ComplexFetchUnwindedNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.symbol, nsum(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        TARGET {  neo4j }";

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
                ("MATCH (pro:progress) UNWIND apoc.convert.fromJsonList(pro.results) as res RETURN res.grade, SUM(res.score) as score").Replace(" ", "")
            );
        }

        [Test]
        public void Convert17_ComplexFetchUnwindedNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.symbol, navg(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        TARGET {  neo4j }";

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
                ("MATCH (pro:progress) UNWIND apoc.convert.fromJsonList(pro.results) as res RETURN res.grade, AVG(res.score) as score").Replace(" ", "")
            );
        }

        [Test]
        public void Convert18_ComplexFetchUnwindedNCount_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.symbol, ncount(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        TARGET {  neo4j }";

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
                ("MATCH (pro:progress) UNWIND apoc.convert.fromJsonList(pro.results) as res RETURN res.grade, COUNT(res.score) as score").Replace(" ", "")
            );
        }

        [Test]
        public void Convert19_ComplexFetchUnwindedNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.symbol, nmin(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        TARGET {  neo4j }";

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
                ("MATCH (pro:progress) UNWIND apoc.convert.fromJsonList(pro.results) as res RETURN res.grade, MIN(res.score) as score").Replace(" ", "")
            );
        }

        [Test]
        public void Convert20_ComplexFetchUnwindedNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.transcript.symbol, nmax(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        TARGET {  neo4j }";

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
                ("MATCH (pro:progress) UNWIND apoc.convert.fromJsonList(pro.results) as res RETURN res.grade, MAX(res.score) as score").Replace(" ", "")
            );
        }

        [Test]
        public void Convert21_ComplexFetchUnwindedNSUMWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, s.register.course.name, nsum(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        FILTER_ON { s.transcript.result > 0 }
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";
            
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
                ("MATCH (cou:course)<-[:ENROLLED_IN]-(pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res WITH pup, cou, res WHERE res.score > 0 RETURN pup.title, pup.name, pup.surname, cou.description, SUM(res.score) as score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert22_ComplexFetchUnwindedNAVGWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, s.register.course.name, navg(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        FILTER_ON { s.transcript.result > 0 }
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";

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
                 ("MATCH (cou:course)<-[:ENROLLED_IN]-(pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res WITH pup, cou, res WHERE res.score > 0 RETURN pup.title, pup.name, pup.surname, cou.description, AVG(res.score) as score LIMIT 10").Replace(" ", "")
             );
        }

        [Test]
        public void Convert23_ComplexFetchUnwindedNCountWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, s.register.course.name, ncount(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        FILTER_ON { s.transcript.result > 0 }
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";

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
                ("MATCH (cou:course)<-[:ENROLLED_IN]-(pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res WITH pup, cou, res WHERE res.score > 0 RETURN pup.title, pup.name, pup.surname, cou.description, COUNT(res.score) as score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert24_ComplexFetchUnwindedNMINWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, s.register.course.name, nmin(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        FILTER_ON { s.transcript.result > 0 }
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";
            
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
                ("MATCH (cou:course)<-[:ENROLLED_IN]-(pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res WITH pup, cou, res WHERE res.score > 0 RETURN pup.title, pup.name, pup.surname, cou.description, MIN(res.score) as score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert25_ComplexFetchUnwindedNMAXWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.title, s.name, s.surname, s.register.course.name, nmax(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        FILTER_ON { s.transcript.result > 0 }
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";

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
                ("MATCH (cou:course)<-[:ENROLLED_IN]-(pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res WITH pup, cou, res WHERE res.score > 0 RETURN pup.title, pup.name, pup.surname, cou.description, MAX(res.score) as score LIMIT 10").Replace(" ", "")
            );
        }

        [Test]
        public void Convert26_ComplexFetchNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, s.transcript.symbol, nsum(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        ORDER_BY { s.name }
                        TARGET {  neo4j }";

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
                (@"MATCH (pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res RETURN pup.name, pup.surname, pup.idnum, res.grade, SUM(res.score) as score ORDER BY pup.name ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Convert27_ComplexFetchNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, s.transcript.symbol, navg(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        ORDER_BY { s.name }
                        TARGET {  neo4j }";

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
                (@"MATCH (pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res RETURN pup.name, pup.surname, pup.idnum, res.grade, AVG(res.score) as score ORDER BY pup.name ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Convert28_ComplexFetchNCount_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, s.transcript.symbol, ncount(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        ORDER_BY { s.name }
                        TARGET {  neo4j }";

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
                (@"MATCH (pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res RETURN pup.name, pup.surname, pup.idnum, res.grade, COUNT(res.score) as score ORDER BY pup.name ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Convert29_ComplexFetchNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, s.transcript.symbol, nmin(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        ORDER_BY { s.name }
                        TARGET {  neo4j }";

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
                (@"MATCH (pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res RETURN pup.name, pup.surname, pup.idnum, res.grade, MIN(res.score) as score ORDER BY pup.name ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Convert30_ComplexFetchNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, s.transcript.symbol, nmax(s.transcript.result) }
                        DATA_MODEL { student AS s}
                        ORDER_BY { s.name }
                        TARGET {  neo4j }";

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
                (@"MATCH (pup:pupil)-[:TRANSCRIPT]->(pro:progress) UNWIND apoc.convert.fromJsonList( pro.results) as res RETURN pup.name, pup.surname, pup.idnum, res.grade, MAX(res.score) as score ORDER BY pup.name ASC").Replace(" ", "")
            );
        }

        [Test]
        public void Convert31_ModifySinglePropertyWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                        PROPERTIES { name = 'Chuck T'}
                        FILTER_ON { idnumber = '62408306136' }
                        TARGET { neo4j }";

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
                ("MATCH ( pup:pupil ) WHERE pup.idnum = \"62408306136\" SET pup.name = \"Chuck T\"").Replace(" ", "")
            );
        }

        [Test]
        public void Convert32_ModifyMultiplePropertiesWithSingleFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                        PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                        FILTER_ON { idnumber = '62408306136' }
                        TARGET { neo4j }";

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
                ("MATCH ( pup:pupil ) WHERE pup.idnum = \"62408306136\" SET pup.name = \"Chuck T\", pup.surname = \"Tylers\"").Replace(" ", "")
            );
        }

        [Test]
        public void Convert33_ModifySinglePropertyWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                        PROPERTIES { name = 'Chuck T'}
                        TARGET { neo4j }";

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
                ("MATCH ( pup:pupil ) SET pup.name = \"Chuck T\"").Replace(" ", "")
            );
        }

        [Test]
        public void Convert34_ModifyMultiplePropertiesWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"MODIFY { student }
                        PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                        TARGET { neo4j }";

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
                ("MATCH ( pup:pupil ) SET pup.name = \"Chuck T\", pup.surname = \"Tylers\"").Replace(" ", "")
            );
        }

        [Test]
        public void Convert35_AddSingleProperty_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                      PROPERTIES { name = 'Chuck T'}
                      TARGET { neo4j }";

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
                ("CREATE ( pup:pupil { name : \"Chuck T\"} )").Replace(" ", "")
            );
        }

        [Test]
        public void Convert36_AddMultiplePropertieWithoutFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"ADD { student }
                      PROPERTIES { name = 'Chuck T', surname = 'Tylers'}
                      TARGET { neo4j }";

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
                 ("CREATE ( pup:pupil { name : \"Chuck T\", surname : \"Tylers\"} )").Replace(" ", "")
            );
        }

        [Test]
        public void Convert37_FetchUnwindedWithoutAlias_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { transcript.symbol, transcript.result }
                        DATA_MODEL { student}
                        RESTRICT_TO { 10 }
                        TARGET {  neo4j }";

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
                ("MATCH (pro:progress ) UNWIND apoc.convert.fromJsonList(pro.results) as res RETURN res.grade, res.score LIMIT 10").Replace(" ", "")
            );
        }
    }
}
