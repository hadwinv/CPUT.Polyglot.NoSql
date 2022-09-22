using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.QueryBuilder.Syntax.wip.DML
{
    public class QueryExpression
    {
        public const string VERB = "GET|ADD|MODIFY|DELETE";

        public const string TARGET = "Neo4j|Cassandra|Redis|MongoDB";

        public const string PRECEDENCE = "Neo4j|Cassandra|Redis|MongoDB";



        //GET [Entity]
        //TARGET [Neo4j|Cassandra|Redis|MongoDB]
        //PRECEDENCE [Neo4j|Cassandra|Redis|MongoDB]
        //FILTER ON [Entity.filter1 = filter1 AND\OR,...,Entity.filtern = filtern]
        //GROUP BY [Entity.col1,...,Entity.coln]
        //RESTRICT [rows:int]
    }
}
