namespace CPUT.Polyglot.NoSql.Schema.Local.MongoDB
{
    public class CourseModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public SubjectModel subject { get; set; }
        public FacultiesModel faculty { get; set; }
    }
}
