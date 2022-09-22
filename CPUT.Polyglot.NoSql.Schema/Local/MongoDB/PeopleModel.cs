namespace CPUT.Polyglot.NoSql.Schema.Local.MongoDB
{
    public class PeopleModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string id_number { get; set; }
        public DateTime date_of_birth { get; set; }
        public ContactModel contact { get; set; }
        public EnrollmentModel register { get; set; }
    }
}
