namespace CPUT.Polyglot.NoSql.Models.Entities.Unified
{
    public class FacultyModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string DepartmentHead { get; set; }
        public string DepartmentAssistant { get; set; }
        public ContactModel Contact { get; set; }
        public List<CourseModel> Courses { get; set; }
    }
}
