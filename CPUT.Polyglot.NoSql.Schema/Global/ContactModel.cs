namespace CPUT.Polyglot.NoSql.Schema.Global
{
    public class ContactModel
    {
        public string Email { get; set; }
        public string CellNumber { get; set; }
        public string HomeNumber { get; set; }
        public List<string> PreferredCommunication { get; set; }
    }
}
