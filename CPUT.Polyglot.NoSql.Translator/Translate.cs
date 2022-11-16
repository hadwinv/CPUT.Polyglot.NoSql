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

            _interpreter.Add((int)Database.REDIS, new RedisPart(_schema.KeyValue()));
            _interpreter.Add((int)Database.CASSANDRA, new CassandraPart(_schema.Columnar()));
            _interpreter.Add((int)Database.MONGODB, new MongoDbPart(_schema.Document()));
            _interpreter.Add((int)Database.NEOJ4, new Neo4jPart(_schema.Graph()));
        }

        public async Task<List<Constructs>> Convert(ConstructPayload payload)
        {
            List<Constructs> constructs = new List<Constructs>();

            List<Task<Constructs>> tasks = new List<Task<Constructs>>();

            //get targeted databases
            var targetExpr = (TargetExpr)payload.BaseExpr.ParseTree.Single(x => x.GetType().Equals(typeof(TargetExpr)));

            //set up and run query generators
            foreach (StorageExpr storage in targetExpr.Value)
            {
                tasks.Add(Task.Factory.StartNew(
                    () =>
                    {
                        return _interpreter.Run(new Enquiry
                        {
                            Database = GetDatabaseTarget(storage.Value),
                            BaseExpr = payload.BaseExpr,
                            Command = payload.Command,
                            Mapper = _schema.Mapper()
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

        private int GetDatabaseTarget(string name)
        {
            int id;

            switch (name.ToLower().Trim())
            {
                case "redis":
                    id = (int)Database.REDIS;
                    break;
                case "cassandra":
                    id = (int)Database.CASSANDRA;
                    break;
                case "mongodb":
                    id = (int)Database.MONGODB;
                    break;
                case "neo4j":
                    id = (int)Database.NEOJ4;
                    break;
                default:
                    id = -1;
                    break;
            }

            return id;
        }

        #endregion

    }
}