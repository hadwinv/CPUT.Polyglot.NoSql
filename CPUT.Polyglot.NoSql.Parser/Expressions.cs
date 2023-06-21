using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using Superpower;
using Superpower.Parsers;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.Syntax.Statement.DML;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using Superpower.Model;
using CPUT.Polyglot.NoSql.Parser.Parsers.Operators;
using CPUT.Polyglot.NoSql.Common.Parsers;
using System.Text.RegularExpressions;

namespace CPUT.Polyglot.NoSql.Parser
{
    public static class Expressions
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
            from x in (
                from ra in Parse.Ref(() => ReferenceAlias).OptionalOrDefault()
                from dot in Parse.Ref(() => Dot).OptionalOrDefault()
                from keyword in Token.EqualTo(Lexicons.PROPERTY).Try()
                from a in Parse.Ref(() => As).OptionalOrDefault()
                from ran in Parse.Ref(() => ReferenceAliasName).OptionalOrDefault()

                select new PropertyExpr(keyword.ToStringValue(),
                                    ra.HasValue ? ra.ToStringValue() : string.Empty,
                                    ran.HasValue ? ran.ToStringValue() : string.Empty)
            ).Try()
            select x;

        private static TokenListParser<Lexicons, TermExpr> Term =
          from x in (
              from ra in Parse.Ref(() => ReferenceAlias).OptionalOrDefault()
              from dot in Parse.Ref(() => Dot).OptionalOrDefault()
              from keyword in Token.EqualTo(Lexicons.TERM).Try()
              from a in Parse.Ref(() => As).OptionalOrDefault()
              from ran in Parse.Ref(() => ReferenceAliasName).OptionalOrDefault()

              select new TermExpr(keyword.ToStringValue(),
                                  ra.HasValue ? ra.ToStringValue() : string.Empty,
                                  ran.HasValue ? ran.ToStringValue() : string.Empty)
          ).Try()
          select x;

        private static TokenListParser<Lexicons, JsonExpr> Json =
             from x in (
                  from ra in Parse.Ref(() => ReferenceAlias).OptionalOrDefault()
                  from dot in Parse.Ref(() => Dot).OptionalOrDefault()
                  from keyword in Token.EqualTo(Lexicons.JSON_PROPERTY).Try()
                  from a in Parse.Ref(() => As).OptionalOrDefault()
                  from ran in Parse.Ref(() => ReferenceAliasName).OptionalOrDefault()

                  select new JsonExpr(keyword.ToStringValue(),
                                      ra.HasValue ? ra.ToStringValue() : string.Empty,
                                      ran.HasValue ? ran.ToStringValue() : string.Empty)
            ).Try()
            select x;

        private static TokenListParser<Lexicons, DataExpr> Data =
           from keyword in Token.EqualTo(Lexicons.DATA)
           from alias in Parse.Ref(() => As).OptionalOrDefault()
           from refm in Parse.Ref(() => ReferenceModel).OptionalOrDefault()
           select new DataExpr(keyword.ToStringValue(), refm.HasValue ? refm.ToStringValue() : string.Empty);

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

        private static TokenListParser<Lexicons, Token<Lexicons>> Separator =
           from s in Token.EqualTo(Lexicons.COMMA)
           select s;

        private static TokenListParser<Lexicons, Token<Lexicons>> Direction =
          from d in Token.EqualTo(Lexicons.ASC).Or(Token.EqualTo(Lexicons.DESC))
          select d;

        private static TokenListParser<Lexicons, Token<Lexicons>> ReferenceAlias =
          from d in Token.EqualTo(Lexicons.REFERENCE_ALIAS)
          select d;

        private static TokenListParser<Lexicons, Token<Lexicons>> Dot =
        from d in Token.EqualTo(Lexicons.DOT)
        select d;

        private static TokenListParser<Lexicons, Token<Lexicons>> As =
          from d in Token.EqualTo(Lexicons.AS)
          select d;

        private static TokenListParser<Lexicons, Token<Lexicons>> ReferenceModel =
         from d in Token.EqualTo(Lexicons.REFERENCE_MODEL)
         select d;

        private static TokenListParser<Lexicons, Token<Lexicons>> ReferenceAliasName =
         from d in Token.EqualTo(Lexicons.REFERENCE_ALIAS_NAME)
         select d;


        private static TokenListParser<Lexicons, Token<Lexicons>> Operators =
         from ops in Token.EqualTo(Lexicons.EQL)
                        .Or(Token.EqualTo(Lexicons.GTE)
                         .Or(Token.EqualTo(Lexicons.GTR)
                          .Or(Token.EqualTo(Lexicons.LSS)
                           .Or(Token.EqualTo(Lexicons.LTE)))))
         select ops;

        private static TokenListParser<Lexicons, BaseExpr> SimpleExpr =
           from expr in OneOf(
               Parse.Ref(() => NumberLiteral).Cast<Lexicons, NumberLiteralExpr, BaseExpr>(),
               Parse.Ref(() => StringLiteral).Cast<Lexicons, StringLiteralExpr, BaseExpr>(),
               Parse.Ref(() => Property).Cast<Lexicons, PropertyExpr, BaseExpr>(),
               Parse.Ref(() => Term.Cast<Lexicons, TermExpr, BaseExpr>()),
               Parse.Ref(() => Json.Cast<Lexicons, JsonExpr, BaseExpr>()),
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

        private static TokenListParser<Lexicons, GroupExpr> GroupPropertiesExpr =
          from s in Parse.Ref(() => Separator).Try().OptionalOrDefault(new Token<Lexicons>(Lexicons.None, TextSpan.None))
          from t in OperatorExpr(s)
          select new GroupExpr(t);

        private static Func<Token<Lexicons>, TokenListParser<Lexicons, OperatorExpr>> OperatorExpr = (Token<Lexicons> cpr) =>
            from cp in Parse.Ref(() => Comparators).Try().OptionalOrDefault(new Token<Lexicons>(Lexicons.None, TextSpan.None))
            from leftTerm in Parse.Ref(() => SimpleExpr).Try()
            from op in Operators
            from rightTerm in Parse.Ref(() => SimpleExpr.Or(Function.Cast<Lexicons, FunctionExpr, BaseExpr>()))
            select new OperatorExpr(leftTerm, rightTerm, OperatorMapping.OperatorMap[op.Kind], OperatorMapping.CompareMap[cpr.Kind]);

        #endregion

        #region Base Declarations

        private static TokenListParser<Lexicons, DeclareExpr> Fetch =
            from keyword in Token.EqualTo(Lexicons.FETCH)
            from columns in (Columns.ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA)))
            select new DeclareExpr(columns);

        private static TokenListParser<Lexicons, DataModelExpr> Add =
            from keyword in Token.EqualTo(Lexicons.ADD)
            from columns in (Data.ManyDelimitedBy(Token.EqualTo(Lexicons.None)))
            select new DataModelExpr(columns);

        private static TokenListParser<Lexicons, DataModelExpr> Modify =
            from keyword in Token.EqualTo(Lexicons.MODIFY)
            from columns in (Data.ManyDelimitedBy(Token.EqualTo(Lexicons.None)))
            select new DataModelExpr(columns);

        #endregion

        #region Properties

        //
        private static TokenListParser<Lexicons, PropertiesExpr> Properties =
            from keyword in Token.EqualTo(Lexicons.PROPERTIES)
            from lb in Token.EqualTo(Lexicons.LEFT_BRACKET).OptionalOrDefault(new Token<Lexicons>(Lexicons.None, TextSpan.None))
            from grp in (
                from lcb in Token.EqualTo(Lexicons.LEFT_CURLY_BRACKET).OptionalOrDefault(new Token<Lexicons>(Lexicons.None, TextSpan.None))
                from clause in GroupPropertiesExpr.Repeat(PropertySetRepeater(keyword.Span.Position.Absolute, keyword.Span.Source))
                from rcb in Token.EqualTo(Lexicons.RIGHT_CURLY_BRACKET).OptionalOrDefault(new Token<Lexicons>(Lexicons.None, TextSpan.None))
                select clause
            ).ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA)).Try()
            from rb in Token.EqualTo(Lexicons.RIGHT_BRACKET).OptionalOrDefault(new Token<Lexicons>(Lexicons.None, TextSpan.None))

            select new PropertiesExpr(grp);

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
            from clause in GroupExpr.Repeat(ConditionRepeater(keyword.Span.Position.Absolute, keyword.Span.Source))
            select new LinkExpr(clause);

        #endregion

        #region Filter

        private static TokenListParser<Lexicons, FilterExpr> Filter =
            from keyword in Token.EqualTo(Lexicons.FILTER_ON)
            from clause in GroupExpr.Repeat(ConditionRepeater(keyword.Span.Position.Absolute, keyword.Span.Source))
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
            select new RestrictExpr(columns.Value);

        #endregion

        #region Order By



        private static TokenListParser<Lexicons, OrderByExpr> OrderBy =
            from keyword in Token.EqualTo(Lexicons.ORDER_BY)
            from columns in (Test.ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA)))
            select new OrderByExpr(columns);

            
        private static TokenListParser<Lexicons, OrderByPropertyExpr> Test =
            from ra in Parse.Ref(() => ReferenceAlias).OptionalOrDefault()
            from dot in Parse.Ref(() => Dot).OptionalOrDefault()
            from property in Property
            from dir in Parse.Ref(() => Direction).OptionalOrDefault()
            select new OrderByPropertyExpr(property.Value,
                                   ra.HasValue ? ra.ToStringValue() : string.Empty,
                                   OperatorMapping.DirectionMap[dir.Kind]);


        #endregion

        #region Target

        private static TokenListParser<Lexicons, TargetExpr> Target =
             from keyword in Token.EqualTo(Lexicons.TARGET)
             from columns in (Storage.ManyDelimitedBy(Token.EqualTo(Lexicons.COMMA)))
             select new TargetExpr(columns);

        #endregion

        #region Helpers

        private static Func<int, string, int> ConditionRepeater = (int position, string token) =>
        {
            var command = token.Substring(position, token.Substring(position).IndexOf("}"));
            var start = command.IndexOf("{");
            var input = command.Substring(start + 1).Trim();

            var delimiter = input.Replace("AND", "Condition").Replace("OR", "Condition");

            if (delimiter.Contains("Condition"))
                return delimiter.Split("Condition").Count();
            else
                return 1;
        };

        private static Func<int, string, int> PropertySetRepeater = (int position, string token) =>
        {
            var command = token.Substring(position, token.Substring(position).IndexOf("}"));
            var start = command.IndexOf("{");
            
            var input = command.Substring(start + 1).Trim();

            var delimiter = input.Replace(",", "separator");

            if (delimiter.Contains("separator"))
                return delimiter.Split("separator").Count();
            else
                return 1;
        };

        private static TokenListParser<Lexicons, T> OneOf<T>(params TokenListParser<Lexicons, T>[] parsers)
        {
            TokenListParser<Lexicons, T> expr = parsers[0].Try();

            foreach (var p in parsers.Skip(1))
                expr = expr.Or(p);

            return expr;
        }

        #endregion

        //fetch
        public static TokenListParser<Lexicons, BaseExpr> FETCH =
              from fetch in Fetch
              from model in DataModel
              from filter in Parse.Ref(() => Filter).OptionalOrDefault()
              from restrict in Parse.Ref(() => Restrict).OptionalOrDefault()
              from order in Parse.Ref(() => OrderBy).OptionalOrDefault()
              from target in Target
              select new Query().BuildExpression(fetch, model, filter, restrict, order, target);

        //add
        public static TokenListParser<Lexicons, BaseExpr> ADD =
             from add in Add
             from properties in Properties
             from filter in Parse.Ref(() => Filter).OptionalOrDefault()
             from restrict in Parse.Ref(() => Restrict).OptionalOrDefault()
             from target in Target
             select new Query().BuildExpression(add, properties, filter, restrict, target);

        //modify
        public static TokenListParser<Lexicons, BaseExpr> MODIFY =
            from modify in Modify
            from properties in Properties
            from filter in Parse.Ref(() => Filter).OptionalOrDefault()
            from restrict in Parse.Ref(() => Restrict).OptionalOrDefault()
            from target in Target
            select new Query().BuildExpression(modify, properties, filter, restrict, target);
    }
}
