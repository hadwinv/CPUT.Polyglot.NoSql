using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Cassandra;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.Text;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions
{
    public class CassandraGenerator : ICassandraVisitor
    {
        private StringBuilder _query;

        public CassandraGenerator(StringBuilder query) => this._query = query;

        public void Visit(QueryPart query)
        {
            foreach (var expr in query.Expressions)
            {
                if (expr is SelectPart)
                {
                    ((SelectPart)expr).Accept(this);
                }
                else if (expr is UpdatePart)
                {
                    ((UpdatePart)expr).Accept(this);
                }
                else if (expr is InsertPart)
                {
                    ((InsertPart)expr).Accept(this);
                }
                else if (expr is ValuesPart)
                {
                    ((ValuesPart)expr).Accept(this);
                }
                else if (expr is FromPart)
                {
                    ((FromPart)expr).Accept(this);
                }
                else if (expr is SetPart)
                {
                    ((SetPart)expr).Accept(this);
                }
                else if (expr is ConditionPart)
                {
                    ((ConditionPart)expr).Accept(this);
                }
                else if (expr is OrderByPart)
                {
                    ((OrderByPart)expr).Accept(this);
                }
                else if (expr is RestrictPart)
                {
                    ((RestrictPart)expr).Accept(this);
                }
            }
        }

        public void Visit(SelectPart part)
        {
            _query.Append(" SELECT ");

            if (part.Properties != null)
            {
                foreach (var property in part.Properties)
                {
                    if (property is PropertyPart)
                    {
                        ((PropertyPart)property).Accept(this);
                    }
                    else if (property is NativeFunctionPart)
                    {
                        ((NativeFunctionPart)property).Accept(this);
                    }
                    else if (property is SeparatorPart)
                    {
                        ((SeparatorPart)property).Accept(this);
                    }
                }
            }
        }

        #region DML

        public void Visit(UpdatePart part)
        {
            _query.Append(" UPDATE ");

            foreach (var expr in part.Parts)
            {
                if (expr is TablePart)
                {
                    ((TablePart)expr).Accept(this);
                }
            }
        }

        public void Visit(InsertPart part)
        {
            _query.Append(" INSERT INTO ");

            foreach (var expr in part.Parts)
            {
                if (expr is TablePart)
                {
                    ((TablePart)expr).Accept(this);
                }
            }
        }

        public void Visit(SetPart part)
        {
            _query.Append(" SET ");

            foreach (var expr in part.Properties)
            {
                if (expr is SetValuePart)
                    ((SetValuePart)expr).Accept(this);
                else if (expr is SeparatorPart)
                    ((SeparatorPart)expr).Accept(this); ;
            }
        }

        public void Visit(SetValuePart part)
        {
            _query.Append(" " + part.Left.Name + " " + part.Operator.Type + " ");

            if (part.Right.Type == "string")
                _query.Append("\"" + part.Right.Name + "\"");
            else
                _query.Append(part.Right.Name);
        }

        public void Visit(ValuesPart part)
        {
            foreach (var expr in part.Properties)
            {
                if (expr is InsertValuePart)
                {
                    ((InsertValuePart)expr).Accept(this);
                }
            }
        }

        public void Visit(InsertValuePart part)
        {
            _query.Append("( ");

            foreach (var expr in part.Left)
            {
                if (expr is PropertyPart)
                {
                    ((PropertyPart)expr).Accept(this);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this); ;
                }
            }
            _query.Append(" ) VALUES ( ");

            foreach (var expr in part.Right)
            {
                if (expr is PropertyPart)
                {
                    var prop = ((PropertyPart)expr);

                    if (prop.Type == "string")
                    {
                        _query.Append("\"");

                        prop.Accept(this);

                        _query.Append("\"");
                    }
                    else
                        prop.Accept(this);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this); ;
                }
            }

            _query.Append(" ) ");
        }

        #endregion

        public void Visit(FromPart part)
        {
            _query.Append(" FROM ");

            foreach (var expr in part.Properties)
            {
                if (expr is TablePart)
                    ((TablePart)expr).Accept(this);
            }
        }

        public void Visit(TablePart part)
        {
            _query.Append(part.Name);
        }
        
        public void Visit(NativeFunctionPart part)
        {
            _query.Append(" " + part.Type + "(");

            part.PropertyPart.Accept(this);

            _query.Append(") as ");

            if (part.PropertyPart is PropertyPart)
                _query.Append(((PropertyPart)part.PropertyPart).Name);
        }

        public void Visit(PropertyPart part)
        {
            _query.Append(part.Name);    
        }

        public void Visit(ConditionPart part)
        {
            _query.Append(" WHERE ");

            foreach (var expr in part.Logic)
            {
                if (expr is LogicalPart)
                    ((LogicalPart)expr).Accept(this);
                else if (expr is ConditionPart)
                    ((ConditionPart)expr).Accept(this);
            }
        }

        public void Visit(LogicalPart part)
        {
            if (!string.IsNullOrEmpty(part.Compare.Type))
                _query.Append(" " + part.Compare.Type + " ");

            _query.Append(part.Left.Name);

            _query.Append(" " + part.Operator.Type + " ");

            if (part.Right.Type == "string")
                _query.Append("\"" + part.Right.Name + "\"");
            else
                _query.Append(part.Right.Name);
        }

        public void Visit(OperatorPart part)
        {
            _query.Append(" " + part.Type + " ");
        }

        public void Visit(ComparePart part)
        {
            _query.Append(" " + part.Type + " ");
        }

        public void Visit(RestrictPart part)
        {
            _query.Append(" LIMIT " + part.Limit.ToString());
        }

        public void Visit(OrderByPart part)
        {
            _query.Append(" ORDER BY " + part.Name + " ");

            part.Direction.Accept(this);
        }

        public void Visit(SeparatorPart part)
        {
            _query.Append(part.Delimiter + " ");
        }

        public void Visit(DirectionPart part)
        {
            if(!string.IsNullOrEmpty(part.Type))
                _query.Append(" " + part.Type + " ");
        }

    }
}
