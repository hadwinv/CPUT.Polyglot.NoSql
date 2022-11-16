namespace CPUT.Polyglot.NoSql.Models._data.prep
{
    public class UProfile
    {
        public int StudentNo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? GraduatedDate { get; set; }
        public UCourse Course { get; set; }

        public UProfile()
        {
            Course = new UCourse();
        }
    }
}
