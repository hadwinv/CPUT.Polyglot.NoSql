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
    public class MongoDbTests
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
        public void Convert02_SimpleFetchWithRestriction_ReturnExecutableQuery()
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
        public void Convert03_SimpleFetchOrderBy_ReturnExecutableQuery()
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
        public void Convert04_SimpleFetchMoreThanOneOrderBy_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { name, surname, idnumber, dateofbirth }
                    DATA_MODEL { student}
                    ORDER_BY { name, surname} 
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
                ("db.getCollection(\"students\").find({},{name : 1, surname : 1, id_number : 1, date_of_birth : 1})._addSpecial(\"$orderby\", { name : 1 , surname : 1})").Replace(" ", "")
            );
        }

        [Test]
        public void Convert05_SimpleFetchOrderByDesc_ReturnExecutableQuery()
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
        public void Convert06_SimpleFetchWithSingleFilter_ReturnExecutableQuery()
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
        public void Convert07_SimpleFetchWithMoreThanOneFilter_ReturnExecutableQuery()
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
        public void Convert08_SimpleFetchWithMoreThanOneANDFilter_ReturnExecutableQuery()
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
        public void Convert09_SimpleFetchWithMoreThanOneOrFilter_ReturnExecutableQuery()
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
        public void Convert10_SimpleFetchWithORANDFilter_ReturnExecutableQuery()
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
        public void Convert11_ComplexFetchUnwindedProperties_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { t.register.subject.name, t.register.subject.duration }
                    DATA_MODEL { student AS t}
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
                ("db.getCollection(\"students\").aggregate([{ $unwind : {path: \"$enroll.subjects\"}},{ $project : { _id: \"$_id\", name : \"$enroll.subjects.name\", duration : \"$enroll.subjects.duration\" }}])").Replace(" ", "")
            );
        }

        [Test]
        public void Convert12_ComplexFetchUnwindedPropertiesWithFilter_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  t.register.subject.name, t.register.subject.duration }
                    DATA_MODEL { student AS t}
                    FILTER_ON { t.register.subject.duration > 0 }
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
                ("db.getCollection(\"students\").aggregate([{ $match : {\"enroll.subjects.duration\": { $gt : NumberLong(0) } }},{ $unwind : {path: \"$enroll.subjects\"}},{ $project : { _id: \"$_id\", name : \"$enroll.subjects.name\", duration : \"$enroll.subjects.duration\" }}])").Replace(" ", "")
            );
        }

        [Test]
        public void Convert13_ComplexFetchWithFilterNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, nsum(s.register.subject.duration) }
                    DATA_MODEL { student AS s}
                    FILTER_ON {s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND s.register.subject.duration > 0 }
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
                "           {\"enroll.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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

        public void Convert14_ComplexFetchWithFilterNCOUNT_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, ncount(s.register.subject.duration) }
                    DATA_MODEL { student AS s}
                    FILTER_ON {s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND s.register.subject.duration > 0 }
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
                "           {\"enroll.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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
        public void Convert15_ComplexFetchWithFilterNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, navg(s.register.subject.duration) }
                    DATA_MODEL { student AS s}
                    FILTER_ON {s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND s.register.subject.duration > 0 }
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
                "           {\"enroll.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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
        public void Convert16_ComplexFetchWithFilterNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, nmin(s.register.subject.duration) }
                    DATA_MODEL { student AS s}
                    FILTER_ON {s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND s.register.subject.duration > 0 }
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
                "           {\"enroll.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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
        public void Convert17_ComplexFetchWithFilterNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  s.name, s.surname, s.idnumber, nmax(s.register.subject.duration) }
                    DATA_MODEL { student AS s}
                    FILTER_ON {s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND s.register.subject.duration > 0 }
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
                "           {\"enroll.subjects.duration\": { $gt : NumberLong(0) } }" +
                "       ]}}," +
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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

        [Test]
        public void Convert18_ComplexFetchNSUM_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, nsum(s.transcript.subject.duration) }
                    DATA_MODEL { student AS s}
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
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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
        public void Convert19_ComplexFetchWithNCOUNT_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, ncount(s.transcript.subject.duration) }
                    DATA_MODEL { student AS s }
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
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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
        public void Convert20_ComplexFetchNAVG_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, navg(s.transcript.subject.duration) }
                    DATA_MODEL { student AS s}
                    RESTRICT_TO { 2 }
                    ORDER_BY { s.name }
                    TARGET {  mongodb }";

            //LINK_ON { s.student_no = t.student_no }
            //GROUP_BY { s.idnumber, s.name, s.surname }

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
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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
        public void Convert21_ComplexFetchNMIN_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, nmin(s.transcript.subject.duration) }
                    DATA_MODEL { student AS s}
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
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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
        public void Convert22_ComplexFetchNMAX_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH { s.name, s.surname, s.idnumber, nmax(s.transcript.subject.duration) }
                    DATA_MODEL { student AS s}
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
                "   { $unwind : {" +
                "       path: \"$enroll.subjects\"}}," +
                "   { $project : { " +
                "       _id: \"$_id\", " +
                "       name : \"$name\", " +
                "       surname : \"$surname\", " +
                "       id_number : \"$id_number\", " +
                "       subjects : \"$enroll.subjects\"}}," +
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

        [Test]
        public void Convert23_ModifySinglePropertyWithSingleFilter_ReturnExecutableQuery()
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
        public void Convert24_ModifyMultiplePropertiesWithSingleFilter_ReturnExecutableQuery()
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
        public void Convert25_ModifySinglePropertyWithoutFilter_ReturnExecutableQuery()
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
        public void Convert26_ModifyMultiplePropertiesWithoutFilter_ReturnExecutableQuery()
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
        public void Convert27_AddSinglePropertyWithoutFilter_ReturnExecutableQuery()
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
        public void Convert28_AddMultiplePropertieWithoutFilter_ReturnExecutableQuery()
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
        public void Convert29_AddMultipleEntries_ReturnExecutableQuery()
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
        public void Convert30_FetchUnwindedPropertiesWithoutAlias_ReturnExecutableQuery()
        {
            List<Constructs> results = null;

            var input = @"FETCH {  register.subject.name, register.subject.duration }
                    DATA_MODEL { student}
                    FILTER_ON { register.subject.duration > 0 }
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
                ("db.getCollection(\"students\").aggregate([{ $match : {\"enroll.subjects.duration\": { $gt : NumberLong(0) } }},{ $unwind : {path: \"$enroll.subjects\"}},{ $project : { _id: \"$_id\", name : \"$enroll.subjects.name\", duration : \"$enroll.subjects.duration\" }}])").Replace(" ", "")
            );
        }

    }
}
