namespace CPUT.Polyglot.NoSql.Schema.Local.Redis
{
    public class UserModel
    {
        public string identity_number { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string preferred_name { get; set; }
        public string user_name { get; set; }
        public string user_pwd { get; set; }
        public string ip_address { get; set; }
        public string device { get; set; }
        public string session_id { get; set; }
        public DateTime login_date { get; set; }
        public DateTime logout_date { get; set; }
    }
}
