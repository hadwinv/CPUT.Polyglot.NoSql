using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Parser
{
    [TestFixture]
    public class BuilderTests
    {
        [Test]
        public void Fetch_SingleProperty_ReturnSyntaxTree()
        {
            var input = @" FETCH { property }
                            DATA_MODEL { data }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
            //tokens.Select(x => x.Kind).Should().Equal(
            //    //fetch
            //    Lexicons.FETCH,
            //    Lexicons.PROPERTY,
            //    //data model
            //    Lexicons.DATA_MODEL,
            //    Lexicons.DATA,
            //    //restrict model
            //    Lexicons.RESTRICT_TO,
            //    Lexicons.NUMBER,
            //    //target model
            //    Lexicons.TARGET,
            //    Lexicons.NAMED_VENDOR
            //);
        }

        [Test]
        public void Fetch_MoreThanOneProperty_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_PropertyWithEqualCondition_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo = propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_PropertyWithGreaterThanCondition_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo > propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_PropertyWithLessThanCondition_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo < propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_PropertyWithGreaterThanAndEqualToCondition_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo >= propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_PropertyWithLessThanAndEqualToCondition_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (propertyo <= propertyo) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_PropertyWithANDCondition_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (property = property) AND (property = property) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_PropertyWithORCondition_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (property = property) OR (property = property) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_PropertyWithANDORCondition_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, property }
                            DATA_MODEL { data }
                            FILTER_ON { (property = property) AND (property = property) OR (property = property) }
                            RESTRICT_TO { 1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_SimpleSumProperty_ReturnSyntaxTree()
        {
            var input = @" FETCH {sum(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_SimpleAverageProperty_ReturnSyntaxTree()
        {
            var input = @" FETCH {avg(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_SimpleCountProperty_ReturnSyntaxTree()
        {
            var input = @" FETCH {count(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_SimpleMinProperty_ReturnSyntaxTree()
        {
            var input = @" FETCH {min(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_SimpleMaxProperty_ReturnSyntaxTree()
        {
            var input = @" FETCH {max(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Fetch_AggregatedPropertyWithGroupBy_ReturnSyntaxTree()
        {
            var input = @" FETCH { property, sum(property), avg(property), count(property), min(property),  max(property) }
                                DATA_MODEL { data }
                                FILTER_ON { (propertyo = propertyo) }
                                GROUP_BY { property }
                                TARGET { storage_type }";

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_SingleRowWithOneProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = 1 }
                            TARGET[ storage_type ]"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_SingleRowWithMoreThanOneProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = 1, colum = 2, colum = 3, colum = '4'}
                            TARGET[ storage_type ]"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_MulipleRowsWithMoreThanOneProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = 1, colum = 2, colum = 3, colum = '4'}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_SingleRowsWithSumProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_MulipleRowsWithSumProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_SingleRowsWithAverageProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_MulipleRowsWithAverageProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_SingleRowsWithCountProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_MulipleRowsWithCountProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_SingleRowsWithMinProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_MulipleRowsWithMinProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_SingleRowsWithMaxProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Add_MulipleRowsWithMaxProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_SingleRowWithMoreThanOneProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = 1, colum = 2, colum = 3, colum = '4'}
                            TARGET[ storage_type ]";
            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_MulipleRowsWithMoreThanOneProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = 1, colum = 2, colum = 3, colum = '4'}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_SingleRowsWithSumProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_MulipleRowsWithSumProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_SingleRowsWithAverageProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            tokens.Select(x => x.Kind).Should().Equal(
                //add
                Lexicons.ADD,
                Lexicons.DATA_MODEL,
                //data model
                Lexicons.PROPERTIES,
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
        public void Modify_MulipleRowsWithAverageProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_SingleRowsWithCountProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_MulipleRowsWithCountProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_SingleRowsWithMinProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_MulipleRowsWithMinProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_SingleRowsWithMaxProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }

        [Test]
        public void Modify_MulipleRowsWithMaxProperty_ReturnSyntaxTree()
        {
            var input = @"ADD { Entity }
                            PROPERTIES { colum = SUM(property)}
                            DATA_MODEL { data, data}
                            LINK_ON { property = property }
                            FILTER ON {Entity.filter1 = filter1 }
                            GROUP_BY { property }
                            TARGET { storage_type }"
                        ;

            var tokens = new Lexer().Tokenize(input);

            Assert.Fail();
        }
    }
}
