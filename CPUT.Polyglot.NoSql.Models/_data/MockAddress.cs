namespace CPUT.Polyglot.NoSql.Models._data
{
    public class MockAddress
    {
        public int Id { get; set; }
        public int StreetNo { get; set; }
        public string Street { get; set; }
        public string StreetAddress { get; set; }
        public string PostalAddress { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public MockLocation Location { get; set; }
    }
}
