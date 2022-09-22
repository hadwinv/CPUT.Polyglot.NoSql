using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using Superpower;
using Superpower.Parsers;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.Syntax.Statement.DML;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using Superpower.Model;
using CPUT.Polyglot.NoSql.Parser.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Parser
{
    public static class Builder
    {

        #region Simple Expressions

        private static TokenListParser<Lexicons, NumberLiteralExpr> NumberLiteral =
           Token.EqualTo(Lexicons.NUMBER)
              .Apply(Numerics.IntegerInt32)
              .Select(n => new NumberLiteralExpr(n));

        private static TokenListParser<Lexicons, StringLiteralExpr> StringLiteral =
            from keyword in Token.EqualTo(Lexicons.STRING)
            select new StringLiteralExpr(keyword.ToStringValue());

        private static TokenListParser<Lexicons, PropertyExpr> Property =
            from keyword in Token.EqualTo(Lexicons.PROPERTY)
            select new PropertyExpr(keyword.ToStringValue());

        private static TokenListParser<Lexicons, TermExpr> Term =
            from keyword in Token.EqualTo(Lexicons.TERM)
            select new TermExpr(keyword.ToStringValue());

        private static TokenListParser<Lexicons, DataExpr> Data =
           from keyword in Token.EqualTo(Lexicons.DATA)
           select new DataExpr(keyword.ToStringValue());

        private static TokenListParser<Lexicons, StorageExpr> Storage =
           from keyword in Token.EqualTo(Lexicons.NAMED_VENDOR)
           select new StorageExpr(keyword.ToStringValue());

        private static TokenListParser<Lexicons, Token<Lexicons>> LeftPExpr =
         from lp in Token.EqualTo(Lexicons.LEFT_PAREN)
         select lp;

        private static TokenListParser<Lexicons, Token<Lexicons>> RightPExpr =
           from lp in Token.EqualTo(Lexicons.RIGHT_PAREN)
           select lp;

        private static TokenListParser<Lexicons, Token<Lexicons>> Comparators =
           from cp in Token.EqualTo(Lexicons.LAND).Or(Token.EqualTo(Lexicons.LOR))
           select cp;

        private static TokenListParser<Lexicons, Token<Lexicons>> Operators =
         from ops in Token.EqualTo(Lexicons.EQL)
                        .Or(Token.EqualTo(Lexicons.GTE)
                         .Or(Token.EqualTo(Lexicons.GTR)
                          .Or(Token.EqualTo(Lexicons.LSS)
                           .Or(Token.EqualTo(Lexicons.LTE)))))
         select ops;

        private static TokenListParser<Lexicons, Token<Lexicons>> Aggregates =
         from agg in Token.EqualTo(Lexicons.SUM)
                        .Or(Token.EqualTo(Lexicons.AVG)
                         .Or(Token.EqualTo(Lexicons.COUNT)
                          .Or(Token.EqualTo(Lexicons.MIN))
                           .Or(Token.EqualTo(Lexicons.MAX))))
         select agg;

        private static TokenListParser<Lexicons, BaseExpr> SimpleExpr =
           from expr in OneOf(
               Parse.Ref(() => NumberLiteral).Cast<Lexicons, NumberLiteralExpr, BaseExpr>(),
               Parse.Ref(() => StringLiteral).Cast<Lexicons, StringLiteralExpr, BaseExpr>(),
               Parse.Ref(() => Property).Cast<Lexicons, PropertyExpr, BaseExpr>(),
               Parse.Ref(() => Term.Cast<Lexicons, TermExpr, BaseExpr>()),
               Parse.Ref(() => Data).Cast<Lexicons, DataExpr, BaseExpr>()
           ).Try()
           select expr;

        #endregion

        #region Complex Expresions

        private static TokenListParser<Lexicons, BaseExpr> Columns =
           from c in Parse.Ref(() => SimpleExpr.Or(Function.Cast<Lexicons, FunctionExpr, BaseExpr>()))
           select c;

        private static TokenListParser<Lexicons, FunctionExpr> Function =
            from agg in Token.Matching<Lexicons>(x => OperatorMapping.AggregateMap.ContainsKey(x), "aggregates_op")
            from lp in Token.EqualTo(Lexicons.LEFT_PAREN)
            from columns in SimpleExpr.ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA))
            from rp in Token.EqualTo(Lexicons.RIGHT_PAREN)
            select new FunctionExpr(columns, OperatorMapping.AggregateMap[agg.Kind]);

        private static TokenListParser<Lexicons, GroupExpr> GroupExpr =
           from cp in Parse.Ref(() => Comparators).Try().OptionalOrDefault(new Token<Lexicons>(Lexicons.None, TextSpan.None))
           from lp in Parse.Ref(() => LeftPExpr).Try().OptionalOrDefault(new Token<Lexicons>(Lexicons.LEFT_PAREN, TextSpan.None))
           from t in OperatorExpr(cp)
           from rp in Parse.Ref(() => RightPExpr).Try().OptionalOrDefault(new Token<Lexicons>(Lexicons.RIGHT_PAREN, TextSpan.None))
           select new GroupExpr(t);

        private static Func<Token<Lexicons>, TokenListParser<Lexicons, OperatorExpr>> OperatorExpr = (Token<Lexicons> token) =>
            from cp in Parse.Ref(() => Comparators).Try().OptionalOrDefault(new Token<Lexicons>(Lexicons.None, TextSpan.None))
            from leftTerm in Parse.Ref(() => Term).Try()
            from op in Operators
            from rightTerm in Parse.Ref(() => Term).Try()
            select new OperatorExpr(op, leftTerm, rightTerm, token);

        #endregion

        #region Base Declarations

        private static TokenListParser<Lexicons, DeclareExpr> Fetch =
            from keyword in Token.EqualTo(Lexicons.FETCH)
            from columns in (Columns.ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA)))
            select new DeclareExpr(columns);

        private static TokenListParser<Lexicons, DeclareExpr> Add =
            from keyword in Token.EqualTo(Lexicons.ADD)
            from columns in (DataModel.ManyDelimitedBy(Token.EqualTo(Lexicons.None)))
            select new DeclareExpr(columns);

        private static TokenListParser<Lexicons, DeclareExpr> Modify =
            from keyword in Token.EqualTo(Lexicons.MODIFY)
            from columns in (DataModel.ManyDelimitedBy(Token.EqualTo(Lexicons.None)))
            select new DeclareExpr(columns);

        #endregion

        #region Properties

        private static TokenListParser<Lexicons, PropertiesExpr> Properties =
            from keyword in Token.EqualTo(Lexicons.PROPERTIES)
            from clause in GroupExpr.Repeat(Repeater(keyword.Span.Position.Absolute, keyword.Span.Source))
            select new PropertiesExpr(clause);

        #endregion

        #region Data model

        private static TokenListParser<Lexicons, DataModelExpr> DataModel =
            from keyword in Token.EqualTo(Lexicons.DATA_MODEL)
            from columns in Data.ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA))
            select new DataModelExpr(columns);

        #endregion

        #region Link on

        private static TokenListParser<Lexicons, LinkExpr> Link =
            from keyword in Token.EqualTo(Lexicons.LINK_ON)
            from clause in GroupExpr.Repeat(Repeater(keyword.Span.Position.Absolute, keyword.Span.Source))
            select new LinkExpr(clause);

        #endregion

        #region Filter

        private static TokenListParser<Lexicons, FilterExpr> Filter =
            from keyword in Token.EqualTo(Lexicons.FILTER_ON)
            from clause in GroupExpr.Repeat(Repeater(keyword.Span.Position.Absolute, keyword.Span.Source))
            select new FilterExpr(clause);

        #endregion

        #region Group by

        private static TokenListParser<Lexicons, BaseExpr> GroupColumns =
            Parse.Ref(() => Property).Cast<Lexicons, PropertyExpr, BaseExpr>();

        private static TokenListParser<Lexicons, GroupByExpr> GroupBy =
                from keyword in Token.EqualTo(Lexicons.GROUP_BY)
                from columns in (GroupColumns.ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA)))
                select new GroupByExpr(columns);

        #endregion

        #region Restrict To

        private static TokenListParser<Lexicons, RestrictExpr> Restrict =
            from keyword in Token.EqualTo(Lexicons.RESTRICT_TO)
            from columns in NumberLiteral
            select new RestrictExpr(columns);

        #endregion

        #region Target

        private static TokenListParser<Lexicons, TargetExpr> Target =
             from keyword in Token.EqualTo(Lexicons.TARGET)
             from columns in (Storage.ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA)))
             select new TargetExpr(columns);

        #endregion

        #region DML

        //Fetch
        public static TokenListParser<Lexicons, BaseExpr> Select =
              from f in Fetch
              from dm in DataModel
              from l in Link
              from c in Filter
              from g in GroupBy
              from r in Restrict
              from t in Target
              select new QuerySyntax().BuildExpression(f, dm, l, c, g, r, t);

        public static TokenListParser<Lexicons, BaseExpr> Insert =
             from a in Add
             from p in Properties
             from t in Target
             select new QuerySyntax().BuildExpression(a, p, t);

        public static TokenListParser<Lexicons, BaseExpr> Update =
            from a in Modify
            from p in Properties
            from dm in DataModel
            from l in Link
            from f in Filter
            from t in Target
            select new QuerySyntax().BuildExpression(a, p, dm, l, f, t);

        #endregion

        #region Helpers

        private static Func<int, string, int> Repeater = (int position, string token) =>
        {
            var command = token.Substring(position, token.Substring(position).IndexOf("}"));
            var start = command.IndexOf("{");
            var input = command.Substring(start + 1).Trim();

            var condition = input.Replace("AND", "Condition").Replace("OR", "Condition");

            if (condition.Contains("Condition"))
                return condition.Split("Condition").Count();
            else
                return 1;
        };

        private static TokenListParser<Lexicons, T> OneOf<T>(params TokenListParser<Lexicons, T>[] parsers)
        {
            TokenListParser<Lexicons, T> expr = parsers[0].Try();

            foreach (var p in parsers.Skip(1))
            {
                expr = expr.Or(p);
            }

            return expr;
        }

        #endregion
    }
}
