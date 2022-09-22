using ExhaustiveMatching;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public static class Operators11
    {
        /// <remarks>
        /// Taken from https://github.com/prometheus/prometheus/blob/4414351576ac27754d9eec71c271171d5c020677/pkg/labels/matcher.go#L24
        /// </remarks>
        public enum LabelMatch
        {
            Equal,
            NotEqual,
            Regexp,
            NotRegexp
        }

        /// <summary>
        /// Describes the cardinality relationship of two Vectors in a binary operation.
        /// </summary>
        public enum VectorMatchCardinality
        {
            /// <summary>
            /// Default matching behaviour- cardinality of both sides of a vector matching expression are equal.
            /// </summary>
            OneToOne,
            /// <summary>
            /// AKA group_left
            /// </summary>
            ManyToOne,
            /// <summary>
            /// AKA group_right
            /// </summary>
            OneToMany
        }

        public enum Binary
        {
            Pow,
            Mul,
            Div,
            Mod,
            Atan2,
            Add,
            Sub,
            Eql,
            Gte,
            Gtr,
            Lte,
            Lss,
            Neq,
            And,
            Unless,
            Or
        }

        public enum Unary
        {
            /// <summary>
            /// Aka plus ('+')
            /// </summary>
            Add,
            /// <summary>
            /// Aka minus ('-')
            /// </summary>
            Sub
        }

        public enum ValueType
        {
            None,
            Scalar,
            Vector,
            Matrix,
            String,
        }

        /// <summary>
        /// The set of binary operators that operate over sets (instant vectors) only.
        /// </summary>
        public static ImmutableHashSet<Binary> BinarySetOperators { get; set; } = new[]
        {
            Binary.And,
            Binary.Or,
            Binary.Unless
        }.ToImmutableHashSet();

        /// <summary>
        /// The set of binary operations that compare expressions. 
        /// </summary>
        /// <remarks>https://github.com/prometheus/prometheus/blob/f103acd5135b8bbe885b17a73dafc7bbb586319c/promql/parser/lex.go#L71</remarks>
        public static ImmutableHashSet<Binary> BinaryComparisonOperators { get; set; } = new[]
        {
            Binary.Gtr,
            Binary.Gte,
            Binary.Lss,
            Binary.Lte,
            Binary.Eql,
            Binary.Neq
        }.ToImmutableHashSet();

        /// <summary>
        /// The set of binary operations that are arithmetic operators. 
        /// </summary>
        /// <remarks>https://prometheus.io/docs/prometheus/latest/querying/operators/#arithmetic-binary-operators</remarks>
        public static ImmutableHashSet<Binary> BinaryArithmeticOperators { get; set; } = new[]
        {
            Binary.Add,
            Binary.Sub,
            Binary.Mod,
            Binary.Mul,
            Binary.Div,
            Binary.Pow
        }.ToImmutableHashSet();

        /// <summary>
        /// Operators are ordered by highest -> lowest precedence. 
        /// </summary>
        public static ImmutableArray<ImmutableHashSet<Binary>> BinaryPrecedence { get; set; } = new[]
        {
            // TODO support right associativity for pow!
            new[] { Binary.Pow },
            new[] { Binary.Mul, Binary.Div, Binary.Atan2, Binary.Mod },
            new[] { Binary.Add, Binary.Sub },
            new[] { Binary.Eql, Binary.Neq, Binary.Gtr, Binary.Gte, Binary.Lss, Binary.Lte },
            new[] { Binary.And, Binary.Unless },
            new[] { Binary.Or }
        }.Select(x => x.ToImmutableHashSet()).ToImmutableArray();

        /// <summary>
        /// Defines the set of all valid aggregator operators (e.g. sum, avg, etc.)
        /// </summary>
        public static ImmutableDictionary<string, AggregateOperator> Aggregates { get; set; } = new[]
        {
            new AggregateOperator("sum"),
            new AggregateOperator("avg"),
            new AggregateOperator("count"),
            new AggregateOperator("min"),
            new AggregateOperator("max"),
            new AggregateOperator("group"),
            new AggregateOperator("stddev"),
            new AggregateOperator("stdvar"),
            new AggregateOperator("topk", (Parser.ValueType?)ValueType.Scalar),
            new AggregateOperator("bottomk", (Parser.ValueType?)ValueType.Scalar),
            new AggregateOperator("count_values", (Parser.ValueType?)ValueType.String),
            new AggregateOperator("quantile", (Parser.ValueType?)ValueType.Scalar)
        }.ToImmutableDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);

        public static string ToPromQl(this Binary op) => op switch
        {
            Binary.Add => "+",
            Binary.Pow => "^",
            Binary.Mul => "*",
            Binary.Div => "/",
            Binary.Mod => "%",
            Binary.Atan2 => "atan2",
            Binary.Sub => "-",
            Binary.Eql => "==",
            Binary.Gte => ">=",
            Binary.Gtr => ">",
            Binary.Lte => "<=",
            Binary.Lss => "<",
            Binary.Neq => "!=",
            Binary.And => "and",
            Binary.Unless => "unless",
            Binary.Or => "or",
            _ => throw ExhaustiveMatch.Failed(op)
        };

        public static string ToPromQl(this Unary op) => op switch
        {
            Unary.Add => "+",
            Unary.Sub => "-",
            _ => throw ExhaustiveMatch.Failed(op)
        };

        public static string ToPromQl(this LabelMatch op) => op switch
        {
            LabelMatch.Equal => "=",
            LabelMatch.NotEqual => "!=",
            LabelMatch.Regexp => "=~",
            LabelMatch.NotRegexp => "!~",
            _ => throw ExhaustiveMatch.Failed(op)
        };

        public static string? ToPromQl(this VectorMatchCardinality op) => op switch
        {
            VectorMatchCardinality.OneToOne => null,
            VectorMatchCardinality.OneToMany => "group_right",
            VectorMatchCardinality.ManyToOne => "group_left",
            _ => throw ExhaustiveMatch.Failed(op)
        };
    }

    public record AggregateOperator(string Name, ValueType? ParameterType = null);
}
