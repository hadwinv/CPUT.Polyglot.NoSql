namespace CPUT.Polyglot.NoSql.Models.Native.MongoDB
{
    public class CourseModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public SubjectModel subject { get; set; }
        public FacultiesModel faculty { get; set; }
    }
}
