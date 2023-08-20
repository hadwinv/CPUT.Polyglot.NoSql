namespace CPUT.Polyglot.NoSql.Models.Native._data.prep.Redis
{
    public class rUser
    {
        public string identity_number { get; set; }
        public string student_number { get; set; }
        public string title { get; set; }
        public string other_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public DateTime birth_date { get; set; }
        public string gender { get; set; }
        public string user_name { get; set; }
        public string psw { get; set; }
        public string ip_address { get; set; }
        public string device { get; set; }
        public string session_id { get; set; }
        public DateTime login_date { get; set; }
        public DateTime logout_date { get; set; }
        public DateTime audit_date { get; set; }
        public string city { get; set; }
        public string country { get; set; }
    }
}
