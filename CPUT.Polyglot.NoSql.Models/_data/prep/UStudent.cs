namespace CPUT.Polyglot.NoSql.Models._data.prep
{
    public class UStudent
    {
        public string Id { get; set; }
        public string IdNumber { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string Language { get; set; }
        public UProfile Profile { get; set; }
        public UAddress Address { get; set; }
        public List<UMarks> Marks { get; set; }
    }
}
