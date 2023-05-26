using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Statement.DML
{
    public class Query 
    {
        public BaseExpr Syntax { get { return _syntax; } }
        private BaseExpr _syntax { get; set; }

        public Query()
        {
            _syntax = new BaseExpr();
        }

        public BaseExpr? BuildExpression(DeclareExpr declare, DataModelExpr dataModel, LinkExpr? link, FilterExpr? filter, GroupByExpr? group, RestrictExpr? restrict, OrderByExpr? order, TargetExpr? target)
        {
            //add
            _syntax.Add(declare);
            _syntax.Add(dataModel);

            if (link != null)
                _syntax.Add(link);

            if (filter != null)
                _syntax.Add(filter);

            if (group != null)
                _syntax.Add(group);

            if (restrict != null)
                _syntax.Add(restrict);

            if (order != null)
                _syntax.Add(order);

            if (target != null)
                _syntax.Add(target);
            
            return Syntax;
        }
        //? 
        public BaseExpr? BuildExpression(DeclareExpr declare, PropertiesExpr? properties,DataModelExpr dataModel, LinkExpr? link, FilterExpr? filter, GroupByExpr? group, RestrictExpr? restrict, TargetExpr? target)
        {
            //add
            _syntax.Add(declare);

            if (properties != null)
                _syntax.Add(properties);

            _syntax.Add(dataModel);

            if (link != null)
                _syntax.Add(link);

            if (filter != null)
                _syntax.Add(filter);

            if (group != null)
                _syntax.Add(group);

            if (restrict != null)
                _syntax.Add(restrict);

            if (target != null)
                _syntax.Add(target);

            return Syntax;
        }

        //add, properties, filter, restrict, target
        public BaseExpr? BuildExpression(DataModelExpr dataModel, PropertiesExpr properties, FilterExpr? filter, RestrictExpr? restrict, TargetExpr? target)
        {
            //add
            _syntax.Add(dataModel);
            _syntax.Add(properties);

            if (filter != null)
                _syntax.Add(filter);

            if (restrict != null)
                _syntax.Add(restrict);

            if (target != null)
                _syntax.Add(target);

            return Syntax;
        }
    }
}
