using CPUT.Polyglot.NoSql.Console.Middleware.Contracts;
using static System.Guid;

namespace CPUT.Polyglot.NoSql.Console.Middleware
{
    public class DefaultOperation :
    ITransientOperation,
    IScopedOperation,
    ISingletonOperation
    {
        public string OperationId { get; } = NewGuid().ToString()[^4..];
    }
}
