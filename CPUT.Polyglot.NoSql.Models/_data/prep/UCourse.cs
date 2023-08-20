namespace CPUT.Polyglot.NoSql.Models._data.prep
{
    public class UCourse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public List<USubject> Subjects { get; set; }
    }
}
