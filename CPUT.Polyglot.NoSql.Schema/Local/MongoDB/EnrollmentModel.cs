namespace CPUT.Polyglot.NoSql.Schema.Local.MongoDB
{
    public class EnrollmentModel
    {
        public int id { get; set; }
        public DateTime registration_date { get; set; }
        public string status { get; set; }
        public CourseModel course { get; set; }
    }
}
