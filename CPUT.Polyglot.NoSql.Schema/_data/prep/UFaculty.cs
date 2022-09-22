namespace CPUT.Polyglot.NoSql.Schema._data.prep
{
    public class UFaculty
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public List<UCourse> Courses { get; set; }

        public UFaculty()
        {
            Courses = new List<UCourse>();
        }
    }
}
