namespace CPUT.Polyglot.NoSql.Models.Native.MongoDB
{
    public class AddressModel
    {
        public int id { get; set; }
        public string street { get; set; }
        public string code { get; set; }
        public string suburb { get; set; }
        public string city { get; set; }
        public string country { get; set; }
    }
}
