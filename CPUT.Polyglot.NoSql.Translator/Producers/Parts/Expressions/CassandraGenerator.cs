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
        StringBuilder Query;

        public CassandraGenerator(StringBuilder query) => this.Query = query;

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

        public void Visit(SelectPart select)
        {
            Query.Append(" SELECT ");

            if (select.Properties != null)
            {
                foreach (var property in select.Properties)
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

        public void Visit(UpdatePart update)
        {
            Query.Append(" UPDATE ");

            foreach (var expr in update.Parts)
            {
                if (expr is TablePart)
                {
                    ((TablePart)expr).Accept(this);
                }
            }
        }

        public void Visit(InsertPart insert)
        {
            Query.Append(" INSERT INTO ");

            foreach (var expr in insert.Parts)
            {
                if (expr is TablePart)
                {
                    ((TablePart)expr).Accept(this);
                }
            }
        }

        public void Visit(FromPart from)
        {
            Query.Append(" FROM ");

            foreach (var expr in from.Properties)
            {
                if (expr is TablePart)
                    ((TablePart)expr).Accept(this);
            }
        }

        public void Visit(TablePart table)
        {
            Query.Append(table.Name);
        }

        public void Visit(PropertyPart property)
        {
            Query.Append(property.Name);    
        }

        public void Visit(ConditionPart condition)
        {
            Query.Append(" WHERE ");

            foreach (var expr in condition.Logic)
            {
                if (expr is LogicalPart)
                    ((LogicalPart)expr).Accept(this);
                else if (expr is ConditionPart)
                    ((ConditionPart)expr).Accept(this);
            }
        }

        public void Visit(LogicalPart logical)
        {
            if (!string.IsNullOrEmpty(logical.Compare.Type))
                Query.Append(" " + logical.Compare.Type + " ");

            Query.Append(logical.Left.Name);

            Query.Append(" " + logical.Operator.Type + " ");

            if (logical.Right.Type == "string")
                Query.Append("\"" + logical.Right.Name + "\"");
            else
                Query.Append(logical.Right.Name);
        }

        public void Visit(RestrictPart restrict)
        {
            Query.Append(" LIMIT " + restrict.Limit.ToString());
        }

        public void Visit(SeparatorPart separator)
        {
            Query.Append(separator.Delimiter + " ");
        }

        public void Visit(OperatorPart @operator)
        {
            Query.Append(" " + @operator.Type + " ");
        }

        public void Visit(ComparePart compare)
        {
            Query.Append(" " + compare.Type + " ");
        }

        public void Visit(SetPart set)
        {
            Query.Append(" SET ");

            foreach (var expr in set.Properties)
            {
                if (expr is SetValuePart)
                    ((SetValuePart)expr).Accept(this);
                else if (expr is SeparatorPart)
                    ((SeparatorPart)expr).Accept(this); ;
            }
        }

        public void Visit(SetValuePart setValue)
        {
            Query.Append(" " + setValue.Left.Name + " " + setValue.Operator.Type + " ");

            if (setValue.Right.Type == "string")
                Query.Append("\"" + setValue.Right.Name + "\"");
            else
                Query.Append(setValue.Right.Name);
        }

        public void Visit(ValuesPart values)
        {
            foreach (var expr in values.Properties)
            {
                if (expr is InsertValuePart)
                {
                    ((InsertValuePart)expr).Accept(this);
                }
            }
        }

        public void Visit(InsertValuePart insertValue)
        {
            Query.Append("( ");
            
            foreach (var expr in insertValue.Left)
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
            Query.Append(" ) VALUES ( ");
            
            foreach (var expr in insertValue.Right)
            {
                if (expr is PropertyPart)
                {
                    var prop = ((PropertyPart)expr);

                    if (prop.Type == "string")
                    {
                        Query.Append("\"");

                        prop.Accept(this);

                        Query.Append("\"");
                    }
                    else
                        prop.Accept(this);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this); ;
                }
            }

            Query.Append(" ) ");
        }

        public void Visit(OrderByPart orderBy)
        {
            Query.Append(" ORDER BY " + orderBy.Name + " ");

            orderBy.Direction.Accept(this);
        }

        public void Visit(DirectionPart direction)
        {
            if(!string.IsNullOrEmpty(direction.Type))
                Query.Append(" " + direction.Type + " ");
        }

        public void Visit(NativeFunctionPart function)
        {
            Query.Append(" " + function.Type + "(");

            function.PropertyPart.Accept(this);

            Query.Append(") as ");

            if (function.PropertyPart is PropertyPart)
                Query.Append(((PropertyPart)function.PropertyPart).Name);
            else if (function.PropertyPart is UnwindPropertyPart)
                Query.Append(((UnwindPropertyPart)function.PropertyPart).Name);
        }
    }
}
