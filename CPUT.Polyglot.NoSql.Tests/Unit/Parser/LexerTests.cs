using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using FluentAssertions;
using NUnit.Framework;
using Superpower;
using System.Linq;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Parser
{
    [TestFixture]
    public class LexerTests
    {
        #region Fetch

        [Test]
        public void Fetch_SingleProperty_ReturnTokens()
        {
            var input = @" FETCH { property }
                            DATA_MODEL { data }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithRestrict_ReturnTokens()
        {
            var input = @" FETCH { property }
                            DATA_MODEL { data }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_MoreThanOneProperty_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithEqualCondition_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo = propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithGreaterThanCondition_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo > propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.GTR,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithLessThanCondition_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo < propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.LSS,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithGreaterThanAndEqualToCondition_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo >= propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.GTE,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithLessThanAndEqualToCondition_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo <= propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.LTE,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithANDCondition_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (property = property) AND (property = property) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                Lexicons.LAND,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithORCondition_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (property = property) OR (property = property) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                Lexicons.LOR,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_PropertyWithANDORCondition_ReturnTokens()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (property = property) AND (property = property) OR (property = property) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                Lexicons.LAND,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                Lexicons.LOR,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //restrict model
                Lexicons.RESTRICT_TO,
                Lexicons.NUMBER,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleSumProperty_ReturnTokens()
        {
            var input = @" FETCH {sum(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.SUM,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,

                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleAverageProperty_ReturnTokens()
        {
            var input = @" FETCH {avg(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.AVG,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,

                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleCountProperty_ReturnTokens()
        {
            var input = @" FETCH {count(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.COUNT,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //target
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleMinProperty_ReturnTokens()
        {
            var input = @" FETCH {min(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.MIN,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //target
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleMaxProperty_ReturnTokens()
        {
            var input = @" FETCH {max(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.MAX,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //target
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleNSumProperty_ReturnTokens()
        {
            var input = @" FETCH {nsum(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.NSUM,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,

                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleNAverageProperty_ReturnTokens()
        {
            var input = @" FETCH {navg(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.NAVG,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,

                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleNCountProperty_ReturnTokens()
        {
            var input = @" FETCH {ncount(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.NCOUNT,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //target
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleNMinProperty_ReturnTokens()
        {
            var input = @" FETCH {nmin(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.NMIN,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //target
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Fetch_SimpleNMaxProperty_ReturnTokens()
        {
            var input = @" FETCH {nmax(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.NMAX,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                 Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //target
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }


        [Test]
        public void Fetch_AggregatedPropertyWithGroupBy_ReturnTokens()
        {
            var input = @" FETCH { property, sum(property), avg(property), count(property), min(property),  max(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                Lexicons.COMMA,
                Lexicons.SUM,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                Lexicons.RIGHT_PAREN,
                Lexicons.COMMA,
                Lexicons.AVG,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                Lexicons.RIGHT_PAREN,
                Lexicons.COMMA,
                Lexicons.COUNT,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                Lexicons.RIGHT_PAREN,
                Lexicons.COMMA,
                Lexicons.MIN,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                Lexicons.RIGHT_PAREN,
                Lexicons.COMMA,
                Lexicons.MAX,
                Lexicons.LEFT_PAREN,
                Lexicons.PROPERTY,
                Lexicons.RIGHT_PAREN,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //filter
                Lexicons.FILTER_ON,
                Lexicons.LEFT_PAREN,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.TERM,
                Lexicons.RIGHT_PAREN,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR);
        }


        [Test]
        public void Fetch_SinglePropertyOrderBy_ReturnTokens()
        {
            var input = @" FETCH { property }
                            DATA_MODEL { data }
                            ORDER_BY { property }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //order
                Lexicons.ORDER_BY,
                Lexicons.PROPERTY,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_SinglePropertyOrderByAsc_ReturnTokens()
        {
            var input = @" FETCH { property }
                            DATA_MODEL { data }
                            ORDER_BY { property ASC }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //order
                Lexicons.ORDER_BY,
                Lexicons.PROPERTY,
                Lexicons.ASC,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }


        [Test]
        public void Fetch_SinglePropertyOrderByDesc_ReturnTokens()
        {
            var input = @" FETCH { property }
                            DATA_MODEL { data }
                            ORDER_BY { property DESC }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.PROPERTY,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                //order
                Lexicons.ORDER_BY,
                Lexicons.PROPERTY,
                Lexicons.DESC,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Fetch_PropertyWithAlias_ReturnTokens()
        {
            var input = @" FETCH { p.property AS p}
                            DATA_MODEL { data AS p}
                            ORDER_BY { p.property DESC }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //fetch
                Lexicons.FETCH,
                Lexicons.REFERENCE_ALIAS,
                Lexicons.DOT,
                Lexicons.PROPERTY,
                Lexicons.AS,
                Lexicons.REFERENCE_ALIAS_NAME,
                //data model
                Lexicons.DATA_MODEL,
                Lexicons.DATA,
                Lexicons.AS,
                Lexicons.REFERENCE_MODEL,
                //order
                Lexicons.ORDER_BY,
                Lexicons.REFERENCE_ALIAS,
                Lexicons.DOT,
                Lexicons.PROPERTY,
                Lexicons.DESC,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }


        #endregion

        #region Add

        [Test]
        public void Add_SingleRowWithOneProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //add
                Lexicons.ADD,
                Lexicons.DATA,
                //properties
                Lexicons.PROPERTIES,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.STRING,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Add_SingleRowWithMoreThanOneProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                         PROPERTIES { colum = 1, colum = 2, colum = 3, colum = '4'}
                         TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //add
                Lexicons.ADD,
                Lexicons.DATA,
                //properties
                Lexicons.PROPERTIES,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.NUMBER,
                Lexicons.COMMA,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.NUMBER,
                Lexicons.COMMA,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.NUMBER,
                Lexicons.COMMA,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.STRING,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Add_MulipleRowsWithMoreThanOneProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                        PROPERTIES { colum = 1, colum = 2, colum = 3, colum = '4'}
                        DATA_MODEL { data, data}
                        FILTER_ON { filter = 5 }
                        TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.NUMBER,
               Lexicons.COMMA,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.NUMBER,
               Lexicons.COMMA,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.NUMBER,
               Lexicons.COMMA,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.NUMBER,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR
           );
        }

        [Test]
        public void Add_SingleRowsWithSumProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = SUM(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON {filter = '2' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.SUM,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_MulipleRowsWithSumProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            FILTER_ON { filter = '3' }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.SUM,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_SingleRowsWithAverageProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = AVG(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '5' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.AVG,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_MulipleRowsWithAverageProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = AVG(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.AVG,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_SingleRowsWithCountProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = COUNT(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '5' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.COUNT,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_MulipleRowsWithCountProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = COUNT(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.COUNT,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_SingleRowsWithMinProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = MIN(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '5' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.MIN,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_MulipleRowsWithMinProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = MIN(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.MIN,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_SingleRowsWithMaxProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = MAX(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '5' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.MAX,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Add_MulipleRowsWithMaxProperty_ReturnTokens()
        {
            var input = @"ADD { Entity }
                          PROPERTIES { colum = MAX(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.ADD,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.MAX,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        #endregion

        #region Modify

        [Test]
        public void Modify_SingleRowWithMoreThanOneProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                         PROPERTIES { colum = 1, colum = 2, colum = 3, colum = '4'}
                         TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //add
                Lexicons.MODIFY,
                Lexicons.DATA,
                //properties
                Lexicons.PROPERTIES,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.NUMBER,
                Lexicons.COMMA,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.NUMBER,
                Lexicons.COMMA,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.NUMBER,
                Lexicons.COMMA,
                Lexicons.TERM,
                Lexicons.EQL,
                Lexicons.STRING,
                //target model
                Lexicons.TARGET,
                Lexicons.NAMED_VENDOR
            );
        }

        [Test]
        public void Modify_MulipleRowsWithMoreThanOneProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                        PROPERTIES { colum = 1, colum = 2, colum = 3, colum = '4'}
                        DATA_MODEL { data, data}
                        FILTER_ON { filter = 5 }
                        TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.NUMBER,
               Lexicons.COMMA,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.NUMBER,
               Lexicons.COMMA,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.NUMBER,
               Lexicons.COMMA,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.NUMBER,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR
           );
        }

        [Test]
        public void Modify_SingleRowsWithSumProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = SUM(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON {filter = '2' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.SUM,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_MulipleRowsWithSumProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            FILTER_ON { filter = '3' }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.SUM,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_SingleRowsWithAverageProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = AVG(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '5' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.AVG,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_MulipleRowsWithAverageProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = AVG(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.AVG,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_SingleRowsWithCountProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = COUNT(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '5' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.COUNT,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_MulipleRowsWithCountProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = COUNT(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.COUNT,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_SingleRowsWithMinProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = MIN(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '5' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.MIN,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_MulipleRowsWithMinProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = MIN(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.MIN,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_SingleRowsWithMaxProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = MAX(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '5' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.MAX,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        [Test]
        public void Modify_MulipleRowsWithMaxProperty_ReturnTokens()
        {
            var input = @"MODIFY { Entity }
                          PROPERTIES { colum = MAX(property)}
                          DATA_MODEL { data, data}
                          FILTER_ON { filter = '1' }
                          TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
               //add
               Lexicons.MODIFY,
               Lexicons.DATA,
               //properties
               Lexicons.PROPERTIES,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.MAX,
               Lexicons.LEFT_PAREN,
               Lexicons.TERM,
               Lexicons.RIGHT_PAREN,
               //data model
               Lexicons.DATA_MODEL,
               Lexicons.DATA,
               Lexicons.COMMA,
               Lexicons.DATA,
               //filter
               Lexicons.FILTER_ON,
               Lexicons.TERM,
               Lexicons.EQL,
               Lexicons.STRING,
               //target model
               Lexicons.TARGET,
               Lexicons.NAMED_VENDOR);
        }

        #endregion

        #region Describe

        #endregion

        #region Create

        #endregion

        #region Alter

        #endregion
    }
}