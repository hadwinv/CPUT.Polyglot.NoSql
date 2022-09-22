namespace CPUT.Polyglot.NoSql.Common.Helpers
{
    public static class Utils
    {
       // private static IReadOnlyDictionary<Lexicons, Binary> BinaryOperatorMap = new Dictionary<Lexicons, Binary>()
       // {
       //     [Lexicons.ADD] = Binary.Add,
       //     [Lexicons.DIV] = Binary.Div,
       //     [Lexicons.EQL] = Binary.Eql,
       //     [Lexicons.GTE] = Binary.Gte,
       //     [Lexicons.GTR] = Binary.Gtr,
       //     [Lexicons.LSS] = Binary.Lss,
       //     [Lexicons.LTE] = Binary.Lte,
       //     [Lexicons.MUL] = Binary.Mul,
       //     [Lexicons.SUB] = Binary.Sub,
       //     [Lexicons.LAND] = Binary.And,
       //     [Lexicons.LOR] = Binary.Or,
       // };

       // public static ImmutableHashSet<Binary> BinaryComparisonOperators { get; set; } = new[]
       //{
       //     Binary.Gtr,
       //     Binary.Gte,
       //     Binary.Lss,
       //     Binary.Lte,
       //     Binary.Eql
       // }.ToImmutableHashSet();

       // public static ImmutableHashSet<Binary> BinaryArithmeticOperators { get; set; } = new[]
       // {
       //     Binary.Add,
       //     Binary.Sub,
       //     Binary.Mul,
       //     Binary.Div
       // }.ToImmutableHashSet();

       // public static ImmutableArray<ImmutableHashSet<Binary>> BinaryPrecedence { get; set; } = new[]
       // {
       //     new[] { Binary.Mul, Binary.Div },
       //     new[] { Binary.Add, Binary.Sub },
       //     new[] { Binary.Eql, Binary.Gtr, Binary.Gte, Binary.Lss, Binary.Lte },
       //     new[] { Binary.And },
       //     new[] { Binary.Or }
       // }.Select(x => x.ToImmutableHashSet()).ToImmutableArray();

       // private static TokenListParser<Lexicons, T> OneOf<T>(params TokenListParser<Lexicons, T>[] parsers)
       // {
       //     TokenListParser<Lexicons, T> expr = parsers[0].Try();

       //     foreach (var p in parsers.Skip(1))
       //     {
       //         expr = expr.Or(p);
       //     }

       //     return expr;
       // }
    }
}
