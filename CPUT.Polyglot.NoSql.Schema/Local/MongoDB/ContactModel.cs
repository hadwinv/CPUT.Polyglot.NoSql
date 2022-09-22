namespace CPUT.Polyglot.NoSql.Schema.Local.MongoDB
{
    public class ContactModel
    {
        public int id { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public AddressModel address { get; set; }
    }
}
