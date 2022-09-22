namespace CPUT.Polyglot.NoSql.Interface.Middleware
{
    public interface IDataFactory
    {
        void LoadKeyValue();
        void LoadColumnar();
        void LoadDocument();
        void LoadGraph();
    }
}
