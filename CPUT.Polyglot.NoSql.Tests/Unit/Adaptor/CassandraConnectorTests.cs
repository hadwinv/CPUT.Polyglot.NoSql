using Cassandra;
using CPUT.Polyglot.NoSql.Delegator.Adaptors;
using Moq;
using NUnit.Framework;

namespace CPUT.Polyglot.NoSql.Tests.Unit.Adaptor
{
    [TestFixture]
    public class CassandraConnectorTests
    {
        private Mock<ISession> _mockSession;
        private Mock<Cassandra.ICluster> _mockCluster;
        private Mock<Cassandra.Cluster> _mockCluster1;
        private Mock<Builder> _mockBuilder;

        private Mock<Session> _mockSession1;
        

        private CassandraBridge _connector;

        [SetUp]
        public void SetUp()
        {
            _mockSession = new Mock<ISession>();
            _mockCluster = new Mock<Cassandra.ICluster>();
            _mockBuilder = new Mock<Builder>();
            _mockCluster1 = new Mock<Cluster>();
            _mockSession1 = new Mock<Session>();

            //_connector = new CassandraConnector(_mockSession.Object, _mockCluster.Object);
        }

        //[Test]
        //public void Cassandra_CreateConnection_ReturnConnector()
        //{
        //    //_cluster = Cluster.Builder()
        //    //        .AddContactPoints("127.0.0.1")
        //    //        .Build();
        //    // Arrange
        //    //_mockBuilder.Setup(x => x.AddContactPoint(It.IsAny<string>()).Build())
        //    //   .Returns(_mockCluster1.Object);

        //    //_mockCluster.Setup(x => x.Configuration.)
        //    //   .Returns(Cluster.Builder()
        //    //        .AddContactPoints("127.0.0.1")
        //    //        .Build());

        //    _mockSession.Setup(x => x.Cluster)
        //        .Returns(_mockCluster.Object);

        //    // Act
        //    var result = _connector.Connect();

        //    // Assert
        //    Assert.IsInstanceOf<ISession>(result);
        //}


        //[Test]
        //public void Cassandra_DisposeConnection_ReturnNoConnector()
        //{
        //    Assert.Fail();
        //}

        
    }
}
