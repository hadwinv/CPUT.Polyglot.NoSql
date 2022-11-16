namespace CPUT.Polyglot.NoSql.Interface.Repos
{
    public interface IDataFactory
    {
        void LoadKeyValue();
        void LoadColumnar();
        void LoadDocument();
        void LoadGraph();
    }
}
