using Amazon.Auth.AccessControlPolicy;
using Amazon.Runtime.Internal.Transform;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using CPUT.Polyglot.NoSql.Models.Translator.Parts;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Models.Views.Unified;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Strategy
{
    public abstract class StrategyPart
    {
        #region Expressions

        public DeclareExpr? DeclareExpr { get; set; }

        public PropertiesExpr? PropertiesExpr { get; set; }

        public DataModelExpr? DataModelExpr { get; set; }

        public FilterExpr? FilterExpr { get; set; }

        public RestrictExpr? RestrictExpr { get; set; }

        public OrderByExpr? OrderByExpr { get; set; }

        public TargetExpr? TargetExpr { get; set; }

        #endregion

        protected bool DoesQueryContainFunction
        {
            get
            {
                if (DeclareExpr != null)
                {
                    dynamic? expr = DeclareExpr.Value.FirstOrDefault(x => x.GetType().Equals(typeof(FunctionExpr))) as FunctionExpr;

                    if (expr != null)
                    {
                        if (expr.Type == AggregateType.NSum || expr.Type == AggregateType.NAvg ||
                            expr.Type == AggregateType.NMin || expr.Type == AggregateType.NMax || expr.Type == AggregateType.NCount)
                            return true;
                    }
                }
                return false;
            }
        }

        protected Codex Codex = new Codex();

        #region Abstract Interfaces

        public abstract OutputPart Fetch();

        public abstract OutputPart Modify();

        public abstract OutputPart Add();

        #endregion

        public OutputPart Query(CreatePart request)
        {
            //set expression parts
            DeclareExpr = (DeclareExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(DeclareExpr)));
            PropertiesExpr = (PropertiesExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(PropertiesExpr)));
            DataModelExpr = (DataModelExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(DataModelExpr)));
            FilterExpr = (FilterExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(FilterExpr)));
            RestrictExpr = (RestrictExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(RestrictExpr)));
            OrderByExpr = (OrderByExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(OrderByExpr)));
            TargetExpr = (TargetExpr?)request.BaseExpr.ParseTree.SingleOrDefault(x => x.GetType().Equals(typeof(TargetExpr)));

            //make a copy of incoming expression request
            if (request.Command == Command.FETCH)
                return Fetch();
            else if (request.Command == Command.ADD)
                return Add();
            else if (request.Command == Command.MODIFY)
                return Modify();

            return new OutputPart();
        }

        protected Link? SearchAndFindPropertyLink(string model, string property, string database)
        {
            return Assistor.USchema
                        .Where(x => x.View.Name == model)
                        .SelectMany(x => x.View.Resources)
                        .Where(x => x.Property == property)
                        .SelectMany(x => x.Link)
                        .Where(x => x.Target == database)
                        .FirstOrDefault();
        }
    }
}
