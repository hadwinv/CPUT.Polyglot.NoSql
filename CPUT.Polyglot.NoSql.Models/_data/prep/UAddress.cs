namespace CPUT.Polyglot.NoSql.Models._data.prep
{
    public class UAddress
    {
        public int Id { get; set; }
        public int StreetNo { get; set; }
        public string Street { get; set; }
        public string StreetAddress { get; set; }
        public string PostalAddress { get; set; }
        public string City { get; set; }
        public ULocation Location { get; set; }

        public UAddress()
        {
            Location = new ULocation();
        }
    }
}
