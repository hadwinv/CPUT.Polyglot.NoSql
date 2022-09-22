namespace CPUT.Polyglot.NoSql.Schema.Local.MongoDB
{
    public class SubjectModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string shortcode { get; set; }
        public int duration { get; set; }
        public decimal cost { get; set; }
    }
}
