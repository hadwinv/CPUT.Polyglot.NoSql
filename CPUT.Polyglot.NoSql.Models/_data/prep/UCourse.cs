namespace CPUT.Polyglot.NoSql.Models._data.prep
{
    public class UCourse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public List<USubject> Subjects { get; set; }

        public UCourse()
        {
            Subjects = new List<USubject>();
        }
    }
}
