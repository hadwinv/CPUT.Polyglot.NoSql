namespace CPUT.Polyglot.NoSql.Models.Native.MongoDB
{
    public class CourseModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<SubjectModel> subjects { get; set; }
        public FacultiesModel faculty { get; set; }
    }
}
