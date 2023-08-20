using CPUT.Polyglot.NoSql.Models._data.prep.MongoDb;

namespace CPUT.Polyglot.NoSql.Models._data
{
    public class MockDataset
    {
        public List<MockCourse> Courses { get; set; }
        public List<MockPerson> Students { get; set; }
    }
}
