using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using CPUT.Polyglot.NoSql.Models;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Delegator
{
    public class Executor : IExecutor
    {
        private IRedisRepo _redisjRepo;
        private ICassandraRepo _cassandraRepo;
        private IMongoDbRepo _mongoRepo;
        private INeo4jRepo _neo4jRepo;

        public Executor(IRedisRepo redisjRepo, ICassandraRepo cassandraRepo, IMongoDbRepo mongoRepo, INeo4jRepo neo4jRepo)
        {
            _redisjRepo = redisjRepo;
            _cassandraRepo = cassandraRepo;
            _mongoRepo = mongoRepo;
            _neo4jRepo = neo4jRepo;
        }

        public async Task<List<Models.Result>> Forward(Command command, Output output)
        {
            var result = new List<Models.Result>();

            var tasks = new List<Task<Models.Result>>();

            //get query targets
            foreach (var target in output.Constructs)
            {

                if (target.Success)
                {
                    //run tasks
                    tasks.Add(Task.Factory.StartNew(
                            () =>
                            {
                                return Action(
                                    new QueryDirective
                                    {
                                        Command = command,
                                        Executable = target.Result.Query,
                                        Codex = target.Result.Codex
                                    },
                                    target.Target);
                            }));
                }
                else
                {
                    result.Add(new Result { 
                        Success = target.Success,
                        Message = target.Message,
                        Source = target.Target,
                        Status = "Failed",
                    });
                }
            }

            //wait for tasks to complete
            await Task.WhenAll(tasks.ToArray());

            foreach (var task in tasks)
                if (task.Result != null)
                    result.Add(task.Result);

            return result;
        }

        private Result Action(QueryDirective directive, Database target)
        {
            var result = new Models.Result();

            if (target == Database.REDIS)
                result = _redisjRepo.Execute(directive);
            else if (target == Database.CASSANDRA)
                result = _cassandraRepo.Execute(directive);
            else if (target == Database.MONGODB)
                result = _mongoRepo.Execute(directive);
            else if (target == Database.NEO4J)
                result = _neo4jRepo.Execute(directive);

            return result;
        }
       
    }
}