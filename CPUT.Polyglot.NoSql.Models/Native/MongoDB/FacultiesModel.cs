namespace CPUT.Polyglot.NoSql.Models.Native.MongoDB
{
    public class FacultiesModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string shortcode { get; set; }
        public string head { get; set; }
        public ContactModel contact { get; set; }
    }
}
