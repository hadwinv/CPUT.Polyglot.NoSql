using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CPUT.Polyglot.NoSql.Parser.Expressions.Functions;
using CPUT.Polyglot.NoSql.Parser.Expressions.Matches;
using CPUT.Polyglot.NoSql.Parser.Expressions.Selectors;
using CPUT.Polyglot.NoSql.Parser.QueryBuilder.Lexicals;
using Superpower;
using Superpower.Display;
using Superpower.Model;
using Superpower.Parsers;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    /// <summary>
    /// Contains parsers for all syntactic components of PromQL expressions.
    /// </summary>
    public static class Querable
    {

        public static TokenListParser<Lexicon, ParsedValue<Operators11.Unary>> UnaryOperator =
                Token.EqualTo(Lexicon.ADD).Select(t => Operators11.Unary.Add.ToParsedValue(t.Span))
                .Or(Token.EqualTo(Lexicon.SUB).Select(t => Operators11.Unary.Sub.ToParsedValue(t.Span)));

        public static TokenListParser<Lexicon, UnaryExpr> UnaryExpr =
                from op in Parse.Ref(() => UnaryOperator)
                from expr in Parse.Ref(() => Expr)
                select new UnaryExpr(op.Value, expr, op.Span.UntilEnd(expr.Span));

        public static TokenListParser<Lexicon, MetricIdentifier> MetricIdentifier =
            from id in Token.EqualTo(Lexicon.METRIC_IDENTIFIER)
                .Or(Token.EqualTo(Lexicon.IDENTIFIER))
                .Or(Token.EqualTo(Lexicon.AGGREGATE_OP).Where(t => Operators11.Aggregates.ContainsKey(t.ToStringValue()), "aggregate_op"))
                .Or(Token.Matching<Lexicon>(t => AlphanumericOperatorTokens.Contains(t), "operator"))
            select new MetricIdentifier(id.ToStringValue(), id.Span);

        public static TokenListParser<Lexicon, LabelMatchers> LabelMatchers =
            from lb in Token.EqualTo(Lexicon.LEFT_BRACE)
            from matchers in (
                from matcherHead in LabelMatcher
                from matcherTail in (
                    from c in Token.EqualTo(Lexicon.COMMA)
                    from m in LabelMatcher
                    select m
                ).Try().Many()
                from comma in Token.EqualTo(Lexicon.COMMA).Optional()
                select new[] { matcherHead }.Concat(matcherTail)
            ).OptionalOrDefault(Array.Empty<LabelMatcher>())
            from rb in Token.EqualTo(Lexicon.RIGHT_BRACE)
            select new LabelMatchers(matchers.ToImmutableArray(), lb.Span.UntilEnd(rb.Span));

        public static TokenListParser<Lexicon, VectorSelector> VectorSelector =
        (
            from m in MetricIdentifier
            from lm in LabelMatchers.AsNullable().OptionalOrDefault()
            select new VectorSelector(m, lm, lm != null ? m.Span!.Value.UntilEnd(lm.Span) : m.Span!)
        ).Or(
            from lm in LabelMatchers
            select new VectorSelector(lm, lm.Span)
        );

        public static TokenListParser<Lexicon, MatrixSelector> MatrixSelector =
            from vs in VectorSelector
            from lb in Token.EqualTo(Lexicon.LEFT_BRACKET)
            from d in Parse.Ref(() => Duration)
            from rb in Token.EqualTo(Lexicon.RIGHT_BRACKET)
            select new MatrixSelector(vs, d, vs.Span!.Value.UntilEnd(rb.Span));

        // Inside of grouping options label names can be recognized as keywords by the lexer. This is a list of keywords that could also be a label name.
        public static TokenListParser<Lexicon, ParsedValue<string>> LabelValueMatcher =
            from id in Token.EqualTo(Lexicon.IDENTIFIER)
                .Or(Token.EqualTo(Lexicon.AGGREGATE_OP).Where(x => Operators11.Aggregates.ContainsKey(x.ToStringValue())))
                .Or(Token.Matching<Lexicon>(t => KeywordAndAlphanumericOperatorTokens.Contains(t), "keyword_or_operator"))
            .Or(Token.EqualTo(Lexicon.OFFSET))
            select new ParsedValue<string>(id.ToStringValue(), id.Span);

        public static TokenListParser<Lexicon, LabelMatcher> LabelMatcher =
            from id in LabelValueMatcher
            from op in MatchOp
            from str in StringLiteral
            select new LabelMatcher(id.Value, op, str, id.Span.UntilEnd(str.Span));

        public static TokenListParser<Lexicon, Operators11.LabelMatch> MatchOp =
            Token.EqualTo(Lexicon.EQL).Select(_ => Operators11.LabelMatch.Equal)
                .Or(
                    Token.EqualTo(Lexicon.NEQ).Select(_ => Operators11.LabelMatch.NotEqual)
                );

        public static TokenListParser<Lexicon, NumberLiteral> Number =
            from s in
                Token.EqualTo(Lexicon.ADD).Or(Token.EqualTo(Lexicon.SUB))
            .OptionalOrDefault(new Token<Lexicon>(Lexicon.ADD, TextSpan.None))
            from n in Token.EqualTo(Lexicon.NUMBER)
            select new NumberLiteral(
                (n.ToStringValue(), s.Kind) switch
                {
                    (var v, Lexicon.ADD) when v.Equals("Inf", StringComparison.OrdinalIgnoreCase) => double.PositiveInfinity,
                    (var v, Lexicon.SUB) when v.Equals("Inf", StringComparison.OrdinalIgnoreCase) => double.NegativeInfinity,
                    (var v, var op) => double.Parse(v) * (op == Lexicon.SUB ? -1.0 : 1.0)
                },
                s.Span.Length > 0 ? s.Span.UntilEnd(n.Span) : n.Span
            );

        public static Regex DurationRegex =
            new Regex("^(([0-9]+)y)?(([0-9]+)w)?(([0-9]+)d)?(([0-9]+)h)?(([0-9]+)m)?(([0-9]+)s)?(([0-9]+)ms)?$",
                RegexOptions.Compiled);

        public static TokenListParser<Lexicon, Duration> Duration =
            Token.EqualTo(Lexicon.DURATION)
                .Select(n =>
                {
                    static TimeSpan ParseComponent(Match m, int index, Func<int, TimeSpan> parser)
                    {
                        if (m.Groups[index].Success)
                            return parser(int.Parse(m.Groups[index].Value));

                        return TimeSpan.Zero;
                    }

                    var match = DurationRegex.Match(n.ToStringValue());
                    if (!match.Success)
                        throw new ParseException($"Invalid duration: {n.ToStringValue()}", n.Position);

                    var ts = TimeSpan.Zero;
                    ts += ParseComponent(match, 2, i => TimeSpan.FromDays(i) * 365);
                    ts += ParseComponent(match, 4, i => TimeSpan.FromDays(i) * 7);
                    ts += ParseComponent(match, 6, i => TimeSpan.FromDays(i));
                    ts += ParseComponent(match, 8, i => TimeSpan.FromHours(i));
                    ts += ParseComponent(match, 10, i => TimeSpan.FromMinutes(i));
                    ts += ParseComponent(match, 12, i => TimeSpan.FromSeconds(i));
                    ts += ParseComponent(match, 14, i => TimeSpan.FromMilliseconds(i));

                    return new Duration(ts, n.Span);
                });



        public static TokenListParser<Lexicon, StringLiteral> StringLiteral =
            Token.EqualTo(Lexicon.STRING)
                .Select(t =>
                {
                    var c = t.Span.ConsumeChar();
                    if (c.Value == '\'')
                        return new StringLiteral('\'', SingleQuoteStringLiteral.Parse(t.Span.ToStringValue()), t.Span);
                    if (c.Value == '"')
                        return new StringLiteral('"', DoubleQuoteStringLiteral.Parse(t.Span.ToStringValue()), t.Span);
                    if (c.Value == '`')
                        return new StringLiteral('`', RawString.Parse(t.Span.ToStringValue()), t.Span);

                    throw new ParseException($"Unexpected string quote", t.Span.Position);
                });



        public static Func<Expr, TokenListParser<Lexicon, OffsetExpr>> OffsetExpr = (expr) =>
        (
            from offset in Token.EqualTo(Lexicon.OFFSET)
            from neg in Token.EqualTo(Lexicon.SUB).Optional()
            from duration in Duration
                // Where needs to be called once the parser has definitely been advanced beyond the initial token (offset)
                // it parses in order for Or() to consider this a partial failure
                .Where(_ =>
                        ValidOffsetExpressions.Contains(expr.GetType()),
                    "offset modifier must be preceded by an instant vector selector or range vector selector or a subquery"
                )
            select new OffsetExpr(
                expr,
                new Duration(new TimeSpan(duration.Value.Ticks * (neg.HasValue ? -1 : 1))),
                expr.Span!.Value.UntilEnd(duration.Span)
            )
        );

        public static TokenListParser<Lexicon, ParenExpression> ParenExpression =
            from lp in Token.EqualTo(Lexicon.LEFT_PAREN)
            from e in Parse.Ref(() => Expr)
            from rp in Token.EqualTo(Lexicon.RIGHT_PAREN)
            select new ParenExpression(e, lp.Span.UntilEnd(rp.Span));

        public static TokenListParser<Lexicon, ParsedValue<Expr[]>> FunctionArgs =
            from lp in Token.EqualTo(Lexicon.LEFT_PAREN).Try()
            from args in Parse.Ref(() => Expr).ManyDelimitedBy(Token.EqualTo(Lexicon.COMMA))
            from rp in Token.EqualTo(Lexicon.RIGHT_PAREN)
            select args.ToParsedValue(lp.Span, rp.Span);

        public static TokenListParser<Lexicon, FunctionCall> FunctionCall =
            from id in Token.EqualTo(Lexicon.IDENTIFIER).Where(x => Functions.Map.ContainsKey(x.ToStringValue())).Try()
            let function = Functions.Map[id.ToStringValue()]
            from args in FunctionArgs
                .Where(a => function.IsVariadic || !function.IsVariadic && function.ArgTypes.Length == a.Value.Length, $"Incorrect number of argument(s) in call to {function.Name}, expected {function.ArgTypes.Length} argument(s)")
                .Where(a => !function.IsVariadic || function.IsVariadic && a.Value.Length >= function.MinArgCount, $"Incorrect number of argument(s) in call to {function.Name}, expected at least {function.MinArgCount} argument(s)")
                // TODO validate "at most" arguments- https://github.com/prometheus/prometheus/blob/7471208b5c8ff6b65b644adedf7eb964da3d50ae/promql/parser/parse.go#L552
            select new FunctionCall(function, args.Value.ToImmutableArray(), id.Span.UntilEnd(args.Span));


        public static TokenListParser<Lexicon, ParsedValue<ImmutableArray<string>>> GroupingLabels =
            from lParen in Token.EqualTo(Lexicon.LEFT_PAREN)
            from labels in LabelValueMatcher.ManyDelimitedBy(Token.EqualTo(Lexicon.COMMA))
            from rParen in Token.EqualTo(Lexicon.RIGHT_PAREN)
            select labels.Select(x => x.Value).ToImmutableArray().ToParsedValue(lParen.Span, rParen.Span);

        public static TokenListParser<Lexicon, ParsedValue<bool>> BoolModifier =
            from b in Token.EqualTo(Lexicon.BOOL).Optional()
            select b.HasValue.ToParsedValue(b?.Span ?? TextSpan.None);

        public static TokenListParser<Lexicon, VectorMatching> OnOrIgnoring =
            from b in BoolModifier
            from onOrIgnoring in Token.EqualTo(Lexicon.ON).Or(Token.EqualTo(Lexicon.IGNORING))
            from onOrIgnoringLabels in GroupingLabels
            select new VectorMatching(
                Operators11.VectorMatchCardinality.OneToOne,
                onOrIgnoringLabels.Value,
                onOrIgnoring.HasValue && onOrIgnoring.Kind == Lexicon.ON,
                ImmutableArray<string>.Empty,
                b.Value,
                b.HasSpan ? b.Span.UntilEnd(onOrIgnoringLabels.Span) : onOrIgnoring.Span.UntilEnd(onOrIgnoringLabels.Span)
            );

        public static Func<Expr, TokenListParser<Lexicon, SubqueryExpr>> SubqueryExpr = (expr) =>
            from lb in Token.EqualTo(Lexicon.LEFT_BRACKET)
            from range in Duration
            from colon in Token.EqualTo(Lexicon.COLON)
            from step in Duration.AsNullable().OptionalOrDefault()
            from rb in Token.EqualTo(Lexicon.RIGHT_BRACKET)
            select new SubqueryExpr(expr, range, step, expr.Span!.Value.UntilEnd(rb.Span));


        public static TokenListParser<Lexicon, VectorMatching> VectorMatching =
            from vectMatching in (
                from vm in OnOrIgnoring
                from grp in Token.EqualTo(Lexicon.GROUP_LEFT).Or(Token.EqualTo(Lexicon.GROUP_RIGHT))
                from grpLabels in GroupingLabels.OptionalOrDefault(ImmutableArray<string>.Empty.ToEmptyParsedValue())
                select vm with
                {
                    MatchCardinality = grp switch
                    {
                        { HasValue: false } => Operators11.VectorMatchCardinality.OneToOne,
                        { Kind: Lexicon.GROUP_LEFT } => Operators11.VectorMatchCardinality.ManyToOne,
                        { Kind: Lexicon.GROUP_RIGHT } => Operators11.VectorMatchCardinality.OneToMany,
                        _ => Operators11.VectorMatchCardinality.OneToOne
                    },
                    Include = grpLabels.Value,
                    Span = vm.Span!.Value.UntilEnd(grpLabels.HasSpan ? grpLabels.Span : grp.Span)
                }
            ).Try().Or(
                from vm in OnOrIgnoring
                select vm
            ).Try().Or(
                from b in BoolModifier
                select new VectorMatching(b.Value) { Span = b.Span }
            )
            select vectMatching;

        private static IReadOnlyDictionary<Lexicon, Operators11.Binary> BinaryOperatorMap = new Dictionary<Lexicon, Operators11.Binary>()
        {
            [Lexicon.ADD] = Operators11.Binary.Add,
            [Lexicon.LAND] = Operators11.Binary.And,
            //[Lexicon.ATAN2] = Operators.Binary.Atan2,
            [Lexicon.DIV] = Operators11.Binary.Div,
            [Lexicon.EQLC] = Operators11.Binary.Eql,
            [Lexicon.GTE] = Operators11.Binary.Gte,
            [Lexicon.GTR] = Operators11.Binary.Gtr,
            [Lexicon.LSS] = Operators11.Binary.Lss,
            [Lexicon.LTE] = Operators11.Binary.Lte,
            [Lexicon.MOD] = Operators11.Binary.Mod,
            [Lexicon.MUL] = Operators11.Binary.Mul,
            [Lexicon.NEQ] = Operators11.Binary.Neq,
            [Lexicon.LOR] = Operators11.Binary.Or,
            [Lexicon.POW] = Operators11.Binary.Pow,
            [Lexicon.SUB] = Operators11.Binary.Sub,
            //[Lexicon.LUNLESS] = Operators.Binary.Unless
        };

        public static TokenListParser<Lexicon, BinaryExpr> BinaryExpr =
            // Sprache doesn't support lef recursive grammars so we have to parse out binary expressions as lists of non-binary expressions 
            from head in Parse.Ref(() => ExprNotBinary)
            from tail in (
                from opToken in Token.Matching<Lexicon>(x => BinaryOperatorMap.ContainsKey(x), "binary_op")
                let op = BinaryOperatorMap[opToken.Kind]
                from vm in VectorMatching.AsNullable().OptionalOrDefault()
                    .Where(x => x is not { ReturnBool: true } || x.ReturnBool && Operators11.BinaryComparisonOperators.Contains(op), "bool modifier can only be used on comparison operators")
                from expr in Parse.Ref(() => ExprNotBinary)
                select (op, vm, expr)
            ).AtLeastOnce()
            select CreateBinaryExpression(head, tail);

        /// <summary>
        /// Creates a binary expression from a collection of two or more operands and one or more operators.
        /// </summary>
        /// <remarks>
        /// This function need to ensure operator precedence is maintained, e.g. 1 + 2 * 3 or 4 should parsed as (1 + (2 * 3)) or 4.
        /// </remarks>
        /// <param name="head">The first operand</param>
        /// <param name="tail">The trailing operators, vector matching + operands</param>
        private static BinaryExpr CreateBinaryExpression(Expr head, (Operators11.Binary op, VectorMatching? vm, Expr expr)[] tail)
        {
            // Just two operands, no need to do any precedence checking
            if (tail.Length == 1)
                return new BinaryExpr(head, tail[0].expr, tail[0].op, tail[0].vm, head.Span!.Value.UntilEnd(tail[0].expr.Span));

            // Three + operands and we need to group subexpressions by precedence. First things first: create linked lists of all our operands and operators
            var operands = new LinkedList<Expr>(new[] { head }.Concat(tail.Select(x => x.expr)));
            var operators = new LinkedList<(Operators11.Binary op, VectorMatching? vm)>(tail.Select(x => (x.op, x.vm)));

            // Iterate through each level of operator precedence, moving from highest -> lowest
            foreach (var precedenceLevel in Operators11.BinaryPrecedence)
            {
                var lhs = operands.First;
                var op = operators.First;

                // While we have operators left to consume, iterate through each operand + operator
                while (op != null)
                {
                    var rhs = lhs!.Next!;

                    // This operator has the same precedence of the current precedence level- create a new binary subexpression with the current operands + operators
                    if (precedenceLevel.Contains(op.Value.op))
                    {
                        var b = new BinaryExpr(lhs.Value, rhs.Value, op.Value.op, op.Value.vm, lhs.Value.Span!.Value.UntilEnd(rhs.Value.Span)); // TODO span matching
                        var bNode = operands.AddBefore(rhs, b);

                        // Remove the previous operands (will replace with our new binary expression)
                        operands.Remove(lhs);
                        operands.Remove(rhs);

                        lhs = bNode;
                        var nextOp = op.Next;

                        // Remove the operator
                        operators.Remove(op);
                        op = nextOp;
                    }
                    else
                    {
                        // Move on to the next operand + operator
                        lhs = rhs;
                        op = op.Next;
                    }
                }
            }

            return (BinaryExpr)operands.Single();
        }

        public static TokenListParser<Lexicon, ParsedValue<(bool without, ImmutableArray<string> labels)>> AggregateModifier =
            from kind in Token.EqualTo(Lexicon.BY).Try()
                .Or(Token.EqualTo(Lexicon.WITHOUT).Try())
            from labels in GroupingLabels
            select (kind.Kind == Lexicon.WITHOUT, labels.Value).ToParsedValue(kind.Span, labels.Span);

        public static TokenListParser<Lexicon, AggregateExpr> AggregateExpr =
            from op in Token.EqualTo(Lexicon.AGGREGATE_OP)
                .Where(x => Operators11.Aggregates.ContainsKey(x.ToStringValue())).Try()
            let aggOps = Operators11.Aggregates[op.ToStringValue()]
            from argsAndMod in (
                from args in FunctionArgs
                from mod in AggregateModifier.OptionalOrDefault(
                    (without: false, labels: ImmutableArray<string>.Empty).ToEmptyParsedValue()
                )
                select (mod, args: args.Value).ToParsedValue(args.Span, mod.HasSpan ? mod.Span : args.Span)
            ).Or(
                from mod in AggregateModifier
                from args in FunctionArgs
                select (mod, args: args.Value).ToParsedValue(mod.Span, args.Span)
            )
            .Where(x => aggOps.ParameterType == null || aggOps.ParameterType != null && x.Value.args.Length == 2, "wrong number of arguments for aggregate expression provided, expected 2, got 1")
            .Where(x => aggOps.ParameterType != null || aggOps.ParameterType == null && x.Value.args.Length == 1, "wrong number of arguments for aggregate expression provided, expected 1, got 2")
            select new AggregateExpr(
                aggOps,
                argsAndMod.Value.args.Length > 1 ? argsAndMod.Value.args[1] : argsAndMod.Value.args[0],
                argsAndMod.Value.args.Length > 1 ? argsAndMod.Value.args[0] : null,
                argsAndMod.Value.mod.Value.labels,
                argsAndMod.Value.mod.Value.without,
                Span: op.Span.UntilEnd(argsAndMod.Span)
            );

        public static TokenListParser<Lexicon, Expr> ExprNotBinary =
             from head in OneOf(
                 // TODO can we optimize order here?
                 Parse.Ref(() => ParenExpression).Cast<Lexicon, ParenExpression, Expr>(),
                 Parse.Ref(() => AggregateExpr).Cast<Lexicon, AggregateExpr, Expr>(),
                 Parse.Ref(() => FunctionCall).Cast<Lexicon, FunctionCall, Expr>(),
                 Parse.Ref(() => Number).Cast<Lexicon, NumberLiteral, Expr>().Try(),
                 Parse.Ref(() => UnaryExpr).Cast<Lexicon, UnaryExpr, Expr>(),
                 Parse.Ref(() => MatrixSelector).Cast<Lexicon, MatrixSelector, Expr>().Try(),
                 Parse.Ref(() => VectorSelector).Cast<Lexicon, VectorSelector, Expr>(),
                 Parse.Ref(() => StringLiteral).Cast<Lexicon, StringLiteral, Expr>()
             )
             from offsetOrSubquery in Parse.Ref(() => OffsetOrSubquery(head))
             select offsetOrSubquery == null ? head : offsetOrSubquery;

        public static Func<Expr, TokenListParser<Lexicon, Expr?>> OffsetOrSubquery = (expr) =>
            (
                from offset in OffsetExpr(expr)
                select (Expr)offset
            ).Or(
                from subquery in SubqueryExpr(expr)
                select (Expr)subquery
            )
            .AsNullable()
            .Or(
                Parse.Return<Lexicon, Expr?>(null)
            );

        public static TokenListParser<Lexicon, Expr> Expr { get; } =
             from head in Parse.Ref(() => BinaryExpr).Cast<Lexicon, BinaryExpr, Expr>().Try().Or(ExprNotBinary)
             from offsetOrSubquery in OffsetOrSubquery(head)
             select offsetOrSubquery == null ? head : offsetOrSubquery;

        /// <summary>
        /// Parse the specified input as a PromQL expression.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="tokenizer">Pass a customized tokenizer. By default, will create a new instance of <see cref="Tokenizer"/>.</param>
        /// <returns></returns>
        public static Expr ParseExpression(string input, Tokenizer? tokenizer = null)
        {
            tokenizer ??= new Tokenizer();
            return Expr.AtEnd().Parse(new TokenList<Lexicon>(
                tokenizer.Tokenize(input).Where(x => x.Kind != Lexicon.COMMENT).ToArray()
            ));
        }

        private static TextParser<string> StringText(char quoteChar) =>
            from open in Character.EqualTo(quoteChar)
            from content in (
                from escape in Character.EqualTo('\\')
                    // Taken from https://github.com/prometheus/prometheus/blob/7471208b5c8ff6b65b644adedf7eb964da3d50ae/promql/parser/lex.go#L554
                from value in Character.In(quoteChar, 'a', 'b', 'f', 'n', 'r', 't', 'v', '\\')
                    .Message("Unexpected escape sequence")
                select (
                    value switch
                    {
                        'a' => '\a',
                        'b' => '\b',
                        'f' => '\f',
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        'v' => '\v',
                        _ => value
                    }
                )
            ).Or(Character.ExceptIn(quoteChar, '\n'))
            .Many()
            from close in Character.EqualTo(quoteChar)
            select new string(content);

        private static TextParser<string> SingleQuoteStringLiteral = StringText('\'');
        private static TextParser<string> DoubleQuoteStringLiteral = StringText('"');


        private static TokenListParser<Lexicon, T> OneOf<T>(params TokenListParser<Lexicon, T>[] parsers)
        {
            TokenListParser<Lexicon, T> expr = parsers[0].Try();

            foreach (var p in parsers.Skip(1))
            {
                expr = expr.Or(p);
            }

            return expr;
        }

        private static readonly HashSet<Lexicon> AlphanumericOperatorTokens =
            FindTokensMatching(attr => attr.Category == "Operator" && Regex.IsMatch(attr.Example!, "^[a-zA-Z0-9]+$"))
            .ToHashSet();

        private static readonly HashSet<Lexicon> KeywordAndAlphanumericOperatorTokens =
            FindTokensMatching(attr => attr.Category == "Keyword")
            .Concat(AlphanumericOperatorTokens)
            .ToHashSet();

        private static IEnumerable<Lexicon> FindTokensMatching(Func<TokenAttribute, bool> predicate) => typeof(Lexicon).GetMembers()
           .Select(enumMember => (enumMember, attr: enumMember.GetCustomAttributes(typeof(TokenAttribute), false).Cast<TokenAttribute>().SingleOrDefault()))
           .Where(x => x.attr != null && predicate(x.attr))
           .Select(x => Enum.Parse<Lexicon>(x.enumMember.Name))
           .ToHashSet();

        private static readonly HashSet<Type> ValidOffsetExpressions = new HashSet<Type>
        {
            typeof(MatrixSelector),
            typeof(VectorSelector),
            typeof(SubqueryExpr),
        };

        private static TextParser<string> RawString =>
           from open in Character.EqualTo('`')
           from content in Character.Except('`').Many()
           from close in Character.EqualTo('`')
           select new string(content);

    }



    //public static class Extensions
    //{
    //    public static ParsedValue<T> ToParsedValue<T>(this T result, TextSpan start, TextSpan end)
    //    {
    //        return new ParsedValue<T>(result, start.UntilEnd(end));
    //    }

    //    public static ParsedValue<T> ToEmptyParsedValue<T>(this T result)
    //    {
    //        return new ParsedValue<T>(result, TextSpan.None);
    //    }

    //    public static TextSpan UntilEnd(this TextSpan @base, TextSpan? next)
    //    {
    //        if (next == null)
    //            return @base;

    //        int absolute1 = next.Value.Position.Absolute + next.Value.Length;
    //        int absolute2 = @base.Position.Absolute;
    //        return @base.First(absolute1 - absolute2);
    //    }

    //    public static ParsedValue<T> ToParsedValue<T>(this T result, TextSpan span)
    //    {
    //        return new ParsedValue<T>(result, span);
    //    }

    //}
}
