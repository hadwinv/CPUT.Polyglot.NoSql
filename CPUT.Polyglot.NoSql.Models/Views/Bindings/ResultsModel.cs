namespace CPUT.Polyglot.NoSql.Models.Views.Bindings
{
    public class ResultsModel
    {
        public string idnumber { get; set; }
        public string title { get; set; }
        public string preferredname { get; set; }
        public string initial { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string dateofbirth { get; set; }
        public string gender { get; set; }
        public string language { get; set; }
        public AddressModel address { get; set; }
        public ContactModel contact { get; set; }
        public RegisterModel register  { get; set; }
        public TranscriptModel transcript { get; set; }

        public ResultsModel()
        {
            address = new AddressModel();
            contact = new ContactModel();
            register = new RegisterModel();
            transcript = new TranscriptModel();
        }
    }
}