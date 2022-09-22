using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using System.Linq.Expressions;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Statement.DML
{
    public class QuerySyntax 
    {
        public BaseExpr? Root { get; }
        private BaseExpr _root { get; set; }

        public QuerySyntax()
        {
            _root = new BaseExpr();
        }

        public BaseExpr? BuildExpression(DeclareExpr declare, DataModelExpr? dataModel, LinkExpr? link, FilterExpr? filter, GroupByExpr? group, RestrictExpr? restrict, TargetExpr? target)
        {
            //add parts
              if (dataModel != null)
                declare.Add(dataModel);

            if (link != null)
                declare.Add(link);

            if (filter != null)
                declare.Add(filter);

            if (group != null)
                declare.Add(group);

            if (restrict != null)
                declare.Add(restrict);

            if (target != null)
                declare.Add(target);
            
            //add to root
            _root.Add(declare);

            return Root;
        }

        public BaseExpr? BuildExpression(DeclareExpr declare, PropertiesExpr? properties, TargetExpr? target)
        {
            //add parts
            if (properties != null)
                declare.Add(properties);

            if (target != null)
                declare.Add(target);

            //add to root
            _root.Add(declare);

            return Root;
        }

        public BaseExpr? BuildExpression(DeclareExpr declare, PropertiesExpr properties, DataModelExpr? dataModel, LinkExpr? link, FilterExpr? filter, TargetExpr? target)
        {
            //add parts
            if (properties != null)
                declare.Add(properties);

            if (dataModel != null)
                declare.Add(dataModel);

            if (link != null)
                declare.Add(link);

            if (filter != null)
                declare.Add(filter);

            if (target != null)
                declare.Add(target);

            //add to root
            _root.Add(declare);

            return Root;
        }
    }
}
