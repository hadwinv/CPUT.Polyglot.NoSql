using Cassandra;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Adaptor
{
    [TestFixture]
    public class ConnectorTests
    {
        [Test]
        public void Redis_CreateConnection_ReturnConnector()
        {
            Assert.Fail();
        }

        [Test]
        public void Cassandra_CreateConnection_ReturnConnector()
        {
            Assert.Fail();
        }

        [Test]
        public void MongoDB_DisposeConnection_ReturnConnector()
        {
            Assert.Fail();
        }

        [Test]
        public void Neo4j_CreateConnection_ReturnConnector()
        {
            Assert.Fail();
        }

        [Test]
        public void Redis_DisposeConnection_ReturnNoConnector()
        {
            Assert.Fail();
        }

        [Test]
        public void Cassandra_DisposeConnection_ReturnNoConnector()
        {
            Assert.Fail();
        }

        [Test]
        public void MongoDB_DisposeConnection_ReturnNoConnector()
        {
            Assert.Fail();
        }

        [Test]
        public void Neo4j_DisposeConnection_ReturnNoConnector()
        {
            Assert.Fail();
        }
    }
}
