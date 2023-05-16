namespace CPUT.Polyglot.NoSql.Models.Native.MongoDB
{
    public class StudentsModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string id_number { get; set; }
        public DateTime date_of_birth { get; set; }
        public ContactModel contact { get; set; }
        public RegisterModel register { get; set; }
    }
}
