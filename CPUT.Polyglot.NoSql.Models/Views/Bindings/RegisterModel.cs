namespace CPUT.Polyglot.NoSql.Models.Views.Bindings
{
    public class RegisterModel
    {
        public FacultyModel faculty { get; set; }
        public CourseModel course { get; set; }
        public SubjectModel subject { get; set; }
        public string studentno { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string type { get; set; }
        public string ipaddress { get; set; }
        public string date { get; set; }
        public string completiondate { get; set; }

        public RegisterModel()
        {
            faculty = new FacultyModel();
            course = new CourseModel();
            subject = new SubjectModel();
        }
    }
}
