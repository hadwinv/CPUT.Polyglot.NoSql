namespace CPUT.Polyglot.NoSql.Models.Entities.Unified
{
    public class RegistrationModel
    {
        public string StudentNo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EnrollmentType { get; set; }
        public DateTime RegisteredDate { get; set; }
        public CourseModel Course { get; set; }
        public List<SubjectModel> Subjects { get; set; }
    }
}
