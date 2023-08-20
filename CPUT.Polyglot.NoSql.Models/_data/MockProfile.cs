using CPUT.Polyglot.NoSql.Models._data.prep.MongoDb;

namespace CPUT.Polyglot.NoSql.Models._data
{
    public class MockProfile
    {
        public int StudentNo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? GraduatedDate { get; set; }
        //public UCourse Course { get; set; }

        public MockCourse Course { get; set; }
    }
}
