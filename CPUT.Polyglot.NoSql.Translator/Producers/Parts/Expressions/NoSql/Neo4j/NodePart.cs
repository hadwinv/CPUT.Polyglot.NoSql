using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j
{
    public class NodePart : IExpression
    {
        internal string Name { get; set; }

        internal string AliasIdentifier { get; set; }

        internal List<RelationshipPart> Relations { get; set; }

        internal string Source { get; set; }

        internal bool ReferenceAliasOnly { get; set; }

        public NodePart() { }

        public NodePart(string name, string source, Relations[] relations)
        {
            Name = name;
            AliasIdentifier = name.Substring(0,4).ToLower();
            Source = source;

            if (relations != null)
            {
                Relations = new List<RelationshipPart>();

                foreach (var relation in relations)
                    Relations.Add(new RelationshipPart(relation.Cardinality, relation.Reference));
            }
        }

        public void Accept(INeo4jVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(ICassandraVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IRedisVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}
