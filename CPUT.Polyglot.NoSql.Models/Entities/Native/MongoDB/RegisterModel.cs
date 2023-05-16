namespace CPUT.Polyglot.NoSql.Models.Native.MongoDB
{
    public class RegisterModel
    {
        public int id { get; set; }
        public DateTime registration_date { get; set; }
        public string status { get; set; }
        public CourseModel course { get; set; }
    }
}
