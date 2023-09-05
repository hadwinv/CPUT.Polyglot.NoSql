using App.Metrics;
using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Interface;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Mapper;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using CPUT.Polyglot.NoSql.Translator;
using CPUT.Polyglot.NoSql.Translator.Events;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Neo4jClient.Cypher;
using NUnit.Framework;
using NUnit.Framework.Internal;
using StackExchange.Redis;
using Superpower;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Translation
{
    [TestFixture]
    public class PolyglotTests
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
        public void Convert01_MoreThanOneTarget_SimpleFetch_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
                .Where(x => x.Target == Utils.Database.NEO4J)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("MATCH (pup:pupil) RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.MONGODB)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("{ find: 'students', projection: {name : 1, surname : 1, id_number : 1, date_of_birth : 1}}").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("SELECT firstname, lastname, idno, dob FROM student;").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("KEYS*").Replace(" ", ""));
        }

        [Test]
        public void Convert02_MoreThanOneTarget_SimpleFetchWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136'}
                    TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
                .Where(x => x.Target == Utils.Database.NEO4J)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("MATCH(pup: pupil) WHERE pup.idnum = \"62408306136\" RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.MONGODB)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("{ find: 'students', filter : {id_number : '62408306136'}, projection: {name : 1, surname : 1, id_number : 1, date_of_birth : 1}}").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\";").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"62408306136\"").Replace(" ", ""));
        }

        [Test]
        public void Convert03_MoreThanOneTarget_SimpleFetchWithMoreThanOneFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    FILTER_ON {idnumber = '62408306136' AND idnumber = '624083061345'}
                    TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
               .Where(x => x.Target == Utils.Database.NEO4J)
               .Select(x => x.Result.Query.Replace(" ", ""))
               .Should().Equal(("MATCH(pup: pupil) WHERE pup.idnum = \"62408306136\" AND pup.idnum = \"624083061345\"   RETURN pup.name, pup.surname, pup.idnum, pup.dob").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.MONGODB)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("{ find: 'students', filter : {$and : [{id_number : '62408306136'},{id_number : '624083061345'}]}, projection: {name : 1, surname : 1, id_number : 1, date_of_birth : 1}}").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"62408306136\" AND idno = \"624083061345\";").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"62408306136\"; GET \"624083061345\"").Replace(" ", ""));
        }

        [Test]
        public void Convert04_MoreThanOneTarget_SimpleFetchWithFilterANDOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                          DATA_MODEL { student}
                          FILTER_ON { idnumber = '35808404617' }
                          ORDER_BY { name} 
                          TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
              .Where(x => x.Target == Utils.Database.NEO4J)
              .Select(x => x.Result.Query.Replace(" ", ""))
              .Should().Equal(("MATCH (pup:pupil) WHERE pup.idnum = \"35808404617\" RETURN pup.name, pup.surname, pup.idnum, pup.dob ORDER BY pup.name ASC").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.MONGODB)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("{ find: 'students', filter : {id_number : '35808404617'}, projection: {name : 1, surname : 1, id_number : 1, date_of_birth : 1}, sort : {name : 1}}").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"35808404617\" ORDER BY firstname  ASC;").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"35808404617\"").Replace(" ", ""));
        }

        [Test]
        public void Convert05_MoreThanOneTarget_SimpleFetchWithFilterANDOrderByANDRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { id, name, surname, idnumber, dateofbirth }
                          DATA_MODEL { student }
                          FILTER_ON { idnumber = '35808404617' }
                          RESTRICT_TO { 10 }
                          ORDER_BY { name} 
                          TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
              .Where(x => x.Target == Utils.Database.NEO4J)
              .Select(x => x.Result.Query.Replace(" ", ""))
              .Should().Equal(("MATCH (pup:pupil) WHERE pup.idnum = \"35808404617\" RETURN pup.name, pup.surname, pup.idnum, pup.dob ORDER BY pup.name ASC LIMIT 10").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.MONGODB)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("{ find: 'students', filter : {id_number : '35808404617'}, projection: {name : 1, surname : 1, id_number : 1, date_of_birth : 1}, sort : {name : 1}, limit : 10}").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("SELECT firstname, lastname, idno, dob FROM student WHERE idno = \"35808404617\" ORDER BY firstname  ASC  LIMIT 10;").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"35808404617\"").Replace(" ", ""));
        }

        [Test]
        public void Convert06_MoreThanOneTarget_FetchAVGWithFilterANDOrderByANDRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, s.register.course.name, navg(s.register.subject.duration) }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '35808404617' }
                          RESTRICT_TO { 10 }
                          ORDER_BY { s.name} 
                          TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
              .Where(x => x.Target == Utils.Database.NEO4J)
              .Select(x => x.Result.Query.Replace(" ", ""))
              .Should().Equal(("MATCH (pup:pupil)-[:ENROLLED_IN]->(cou:course)-[:CONTAINS]->(sub:subject) WHERE pup.idnum = \"35808404617\" RETURN pup.name, pup.surname, pup.idnum, cou.description, AVG(sub.term) as term ORDER BY pup.name ASC LIMIT 10").Replace(" ", ""));

            var output = String.Compare(@"{ 
                    aggregate: 'students', 
                    pipeline: [ 
                        { $match : { id_number : '35808404617' }},
                        { $unwind : {path: '$enroll.subject'}},
                        { $project : { _id: '$_id', name : '$name', surname : '$surname', id_number : '$id_number', coursename : '$enroll.course.name', subject : '$enroll.subject'}},
                        { $group : { _id: '$_id', name : { '$first' : '$name'}, surname : { '$first' : '$surname'}, id_number : { '$first' : '$id_number'}, coursename : { '$first' : '$coursename'}, duration: { $avg: '$subject.duration'}}},
                        { $sort: {name  : 1 }},
                        { $limit: 10}
                    ],
                    cursor: { }}", results[1].Result.Query, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols);

            output.Should().Be(0);

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal((" SELECT firstname, lastname, idno, registered.course,  AVG(registered.subject.period) as period FROM student WHERE idno = \"35808404617\" ORDER BY firstname  ASC  LIMIT 10;").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"35808404617\"").Replace(" ", ""));
        }

        [Test]
        public void Convert07_MoreThanOneTarget_FetchSUMWithFilterANDOrderByANDRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, s.register.course.name, nsum(s.register.subject.duration) }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '35808404617' }
                          RESTRICT_TO { 10 }
                          ORDER_BY { s.name} 
                          TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
                .Where(x => x.Target == Utils.Database.NEO4J)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("MATCH (pup:pupil)-[:ENROLLED_IN]->(cou:course)-[:CONTAINS]->(sub:subject) WHERE pup.idnum = \"35808404617\" RETURN pup.name, pup.surname, pup.idnum, cou.description, SUM(sub.term) as term ORDER BY pup.name ASC LIMIT 10").Replace(" ", ""));

            var output = String.Compare(@"{ 
                    aggregate: 'students', 
                    pipeline: [ 
                        { $match : { id_number : '35808404617' }},
                        { $unwind : {path: '$enroll.subject'}},
                        { $project : { _id: '$_id', name : '$name', surname : '$surname', id_number : '$id_number', coursename : '$enroll.course.name', subject : '$enroll.subject'}},
                        { $group : { _id: '$_id', name : { '$first' : '$name'}, surname : { '$first' : '$surname'}, id_number : { '$first' : '$id_number'}, coursename : { '$first' : '$coursename'}, duration: { $sum: '$subject.duration'}}},
                        { $sort: {name  : 1 }},
                        { $limit: 10}
                    ],
                    cursor: { }}", results[1].Result.Query, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols);

            output.Should().Be(0);

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal((" SELECT firstname, lastname, idno, registered.course,  SUM(registered.subject.period) as period FROM student WHERE idno = \"35808404617\" ORDER BY firstname  ASC  LIMIT 10;").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"35808404617\"").Replace(" ", ""));
        }

        [Test]
        public void Convert08_MoreThanOneTarget_FetchMINWithFilterANDOrderByANDRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, s.register.course.name, nmin(s.register.subject.duration) }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '35808404617' }
                          RESTRICT_TO { 10 }
                          ORDER_BY { s.name} 
                          TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
                .Where(x => x.Target == Utils.Database.NEO4J)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("MATCH (pup:pupil)-[:ENROLLED_IN]->(cou:course)-[:CONTAINS]->(sub:subject) WHERE pup.idnum = \"35808404617\" RETURN pup.name, pup.surname, pup.idnum, cou.description, MIN(sub.term) as term ORDER BY pup.name ASC LIMIT 10").Replace(" ", ""));

            var output = String.Compare(@"{ 
                    aggregate: 'students', 
                    pipeline: [ 
                        { $match : { id_number : '35808404617' }},
                        { $unwind : {path: '$enroll.subject'}},
                        { $project : { _id: '$_id', name : '$name', surname : '$surname', id_number : '$id_number', coursename : '$enroll.course.name', subject : '$enroll.subject'}},
                        { $group : { _id: '$_id', name : { '$first' : '$name'}, surname : { '$first' : '$surname'}, id_number : { '$first' : '$id_number'}, coursename : { '$first' : '$coursename'}, duration: { $min: '$subject.duration'}}},
                        { $sort: {name  : 1 }},
                        { $limit: 10}
                    ],
                    cursor: { }}", results[1].Result.Query, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols);

            output.Should().Be(0);

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal((" SELECT firstname, lastname, idno, registered.course,  MIN(registered.subject.period) as period FROM student WHERE idno = \"35808404617\" ORDER BY firstname  ASC  LIMIT 10;").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"35808404617\"").Replace(" ", ""));
        }

        [Test]
        public void Convert09_MoreThanOneTarget_FetchMAXWithFilterANDOrderByANDRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, s.register.course.name, nmax(s.register.subject.duration) }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '35808404617' }
                          RESTRICT_TO { 10 }
                          ORDER_BY { s.name} 
                          TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
                .Where(x => x.Target == Utils.Database.NEO4J)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("MATCH (pup:pupil)-[:ENROLLED_IN]->(cou:course)-[:CONTAINS]->(sub:subject) WHERE pup.idnum = \"35808404617\" RETURN pup.name, pup.surname, pup.idnum, cou.description, MAX(sub.term) as term ORDER BY pup.name ASC LIMIT 10").Replace(" ", ""));

            var output = String.Compare(@"{ 
                    aggregate: 'students', 
                    pipeline: [ 
                        { $match : { id_number : '35808404617' }},
                        { $unwind : {path: '$enroll.subject'}},
                        { $project : { _id: '$_id', name : '$name', surname : '$surname', id_number : '$id_number', coursename : '$enroll.course.name', subject : '$enroll.subject'}},
                        { $group : { _id: '$_id', name : { '$first' : '$name'}, surname : { '$first' : '$surname'}, id_number : { '$first' : '$id_number'}, coursename : { '$first' : '$coursename'}, duration: { $max: '$subject.duration'}}},
                        { $sort: {name  : 1 }},
                        { $limit: 10}
                    ],
                    cursor: { }}", results[1].Result.Query, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols);

            output.Should().Be(0);

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal((" SELECT firstname, lastname, idno, registered.course,  MAX(registered.subject.period) as period FROM student WHERE idno = \"35808404617\" ORDER BY firstname  ASC  LIMIT 10;").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"35808404617\"").Replace(" ", ""));
        }

        [Test]
        public void Convert10_MoreThanOneTarget_FetchCOUNTWithFilterANDOrderByANDRestriction_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, s.register.course.name, ncount(s.register.subject.duration) }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '35808404617' }
                          RESTRICT_TO { 10 }
                          ORDER_BY { s.name} 
                          TARGET {  neo4j, mongodb, cassandra, redis }";

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

            results
                .Where(x => x.Target == Utils.Database.NEO4J)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("MATCH (pup:pupil)-[:ENROLLED_IN]->(cou:course)-[:CONTAINS]->(sub:subject) WHERE pup.idnum = \"35808404617\" RETURN pup.name, pup.surname, pup.idnum, cou.description, COUNT(sub.term) as term ORDER BY pup.name ASC LIMIT 10").Replace(" ", ""));

            var output = String.Compare(@"{ 
                    aggregate: 'students', 
                    pipeline: [ 
                        { $match : { id_number : '35808404617' }},
                        { $unwind : {path: '$enroll.subject'}},
                        { $project : { _id: '$_id', name : '$name', surname : '$surname', id_number : '$id_number', coursename : '$enroll.course.name', subject : '$enroll.subject'}},
                        { $group : { _id: '$_id', name : { '$first' : '$name'}, surname : { '$first' : '$surname'}, id_number : { '$first' : '$id_number'}, coursename : { '$first' : '$coursename'}, duration: { $count: {}}}},
                        { $sort: {name  : 1 }},
                        { $limit: 10}
                    ],
                    cursor: { }}", results[1].Result.Query, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols);

            output.Should().Be(0);

            results
                .Where(x => x.Target == Utils.Database.CASSANDRA)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal((" SELECT firstname, lastname, idno, registered.course,  COUNT(registered.subject.period) as period FROM student WHERE idno = \"35808404617\" ORDER BY firstname  ASC  LIMIT 10;").Replace(" ", ""));

            results
                .Where(x => x.Target == Utils.Database.REDIS)
                .Select(x => x.Result.Query.Replace(" ", ""))
                .Should().Equal(("GET \"35808404617\"").Replace(" ", ""));
        }

    }
}
