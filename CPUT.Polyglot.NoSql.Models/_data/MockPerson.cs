namespace CPUT.Polyglot.NoSql.Models._data
{
    public class MockPerson
    {
        //public string Id { get; set; }
        //public string IdNumber { get; set; }
        //public string Title { get; set; }
        //public string Name { get; set; }
        //public string Surname { get; set; }
        //public string DOB { get; set; }
        //public string Gender { get; set; }
        //public string Email { get; set; }
        //public string MobileNo { get; set; }
        //public string Language { get; set; }
        //public UProfile Profile { get; set; }
        //public UAddress Address { get; set; }
        //public List<UMarks> Marks { get; set; }

        public string ID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string IDNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string IPAddress { get; set; }
        public string DOB { get; set; }
        public string CreditCardNo { get; set; }
        public string StreetAddress { get; set; }
        public string Language { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string PostalAddress { get; set; }
        public string ApplicationName { get; set; }
        public string ProfileId { get; set; }

        public MockProfile Profile { get; set; }
        public MockAddress Address { get; set; }
    }
}
