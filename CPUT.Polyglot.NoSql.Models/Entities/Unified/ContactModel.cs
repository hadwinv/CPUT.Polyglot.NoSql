namespace CPUT.Polyglot.NoSql.Models.Entities.Unified
{
    public class ContactModel
    {
        public string Email { get; set; }
        public string CellNumber { get; set; }
        public string HomeNumber { get; set; }
        public List<string> PreferredCommunication { get; set; }
    }
}
