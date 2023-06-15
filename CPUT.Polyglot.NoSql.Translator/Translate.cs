using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator.Events;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Translator
{
    public class Translate : ITranslate
    {
        private IInterpreter _interpreter;
        private ISchema _schema;

        public Translate(IInterpreter interpreter, ISchema schema)
        {
            _interpreter = interpreter;
            _schema = schema;

            //global schemas
            Assistor.USchema = _schema.UnifiedView();
            //native schemas
            Assistor.Add((int)Database.REDIS, _schema.KeyValue());
            Assistor.Add((int)Database.CASSANDRA, _schema.Columnar());
            Assistor.Add((int)Database.NEO4J, _schema.Graph());
            Assistor.Add((int)Database.MONGODB, _schema.Document());

            _interpreter.Add(Database.REDIS, new RedisPart());
            _interpreter.Add(Database.CASSANDRA, new CassandraPart());
            _interpreter.Add(Database.NEO4J, new Neo4jPart());
            _interpreter.Add(Database.MONGODB, new MongoDbPart());
        }

        public async Task<List<Constructs>> Convert(ConstructPayload payload)
        {
            List<Constructs> constructs = new List<Constructs>();
            List<Task<Constructs>> tasks = new List<Task<Constructs>>();

            //get targeted databases
            var targetExpr = (TargetExpr)payload.BaseExpr.ParseTree.Single(x => x.GetType().Equals(typeof(TargetExpr)));

            //var targetExpr = payload.BaseExpr.ParseTree.Where(x => x.GetType().Equals(typeof(TargetExpr))).Select(x => (TargetExpr)x.v);
            //set up and run query generators
            foreach (StorageExpr storage in targetExpr.Value)
            {
                tasks.Add(Task.Factory.StartNew(
                    () =>
                    {
                        return _interpreter.Run(new Enquiry
                        {
                            BaseExpr = payload.BaseExpr,
                            Command = payload.Command,
                            Database = GetDatabaseTarget(storage.Value),
                        });
                    }));
            }

            await Task.WhenAll(tasks.ToArray());

            foreach (var task in tasks)
                if(task.Result != null)
                    constructs.Add(task.Result);

            return constructs;
        }

        #region private methods

        private Database GetDatabaseTarget(string name)
        {
            Database db;

            switch (name.ToLower().Trim())
            {
                case "redis":
                    db = Database.REDIS;
                    break;
                case "cassandra":
                    db = Database.CASSANDRA;
                    break;
                case "mongodb":
                    db = Database.MONGODB;
                    break;
                case "neo4j":
                    db = Database.NEO4J;
                    break;
                default:
                    db = Database.NONE;
                    break;
            }

            return db;
        }

        #endregion

    }
}