using CPUT.Polyglot.NoSql.Parser.Builder.Lexicals;
using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Expressions.Functions
{
    public record FunctionCall(Function Function, ImmutableArray<Expr> Args, TextSpan? Span = null) : Expr
    {
        public FunctionCall(Function function, params Expr[] args)
            : this(function, args.ToImmutableArray())
        {
        }

        public Function Function { get; set; } = Function;
        public ImmutableArray<Expr> Args { get; set; } = Args;

        public ValueType Type => Function.ReturnType;

        public void Accept(IVisitor visitor) => visitor.Visit(this);

        public Expr DeepClone() => this with { Args = Args.Select(a => a.DeepClone()).ToImmutableArray() };

        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.AppendLine($"{nameof(Function)} = {Function.Name}, ");
            builder.Append($"{nameof(Args)} = ");
            Args.PrintArray(builder);

            return true;
        }
    }
}

