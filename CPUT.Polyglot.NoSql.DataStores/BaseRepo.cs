namespace CPUT.Polyglot.NoSql.DataStores
{
    public abstract class BaseRepo
    {
        private readonly ConnectionStrings _connectionStrings;

        public BaseRepo(ConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }

        protected virtual string KeyConnectionString
        {
            get
            {
                return _connectionStrings.KeyConnectionString; // specifies a specific connection string
            }
        }

        protected virtual string ColumnarConnectionString
        {
            get
            {
                return _connectionStrings.ColumnarConnectionString; // specifies a specific connection string
            }
        }

        protected virtual string DocumentConnectionString
        {
            get
            {
                return _connectionStrings.DocumentConnectionString; // specifies a specific connection string
            }
        }

        protected virtual string GraphConnectionString
        {
            get
            {
                return _connectionStrings.GraphConnectionString; // specifies a specific connection string
            }
        }

        protected virtual int DefaultTimeOut
        {
            get
            {
                return _connectionStrings.Timeout;
            }
        }

    }
}
