namespace CPUT.Polyglot.NoSql.Models.Native.MongoDB
{
    public class ContactModel
    {
        public int id { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public AddressModel address { get; set; }
    }
}
