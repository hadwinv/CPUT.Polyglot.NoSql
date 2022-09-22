namespace CPUT.Polyglot.NoSql.DataStores
{
    public class ConnectionStrings
    {
        public string KeyConnectionString { get; set; }

        public string ColumnarConnectionString { get; set; }

        public string DocumentConnectionString { get; set; }

        public string GraphConnectionString { get; set; }

        public int Timeout { get; set; }
    }
}
