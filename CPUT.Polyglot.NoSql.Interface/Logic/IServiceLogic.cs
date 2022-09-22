namespace CPUT.Polyglot.NoSql.Interface.Logic
{
    public interface IServiceLogic
    {
        string LoadMockData();

        string Query(string statement);
    }
}
