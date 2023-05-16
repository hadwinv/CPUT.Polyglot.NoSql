using CPUT.Polyglot.NoSql.Common;
using CPUT.Polyglot.NoSql.DataStores.Repos.Columnar;
using CPUT.Polyglot.NoSql.DataStores.Repos.Document;
using CPUT.Polyglot.NoSql.DataStores.Repos.Graph;
using CPUT.Polyglot.NoSql.DataStores.Repos.KeyValue;
using CPUT.Polyglot.NoSql.Delegator.Adaptors;
using CPUT.Polyglot.NoSql.Interface;
using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Logic;
using CPUT.Polyglot.NoSql.Logic.Core;
using CPUT.Polyglot.NoSql.Logic.Core.Events;
using CPUT.Polyglot.NoSql.Mapper;
using CPUT.Polyglot.NoSql.Translator;
using CPUT.Polyglot.NoSql.Translator.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proxy = CPUT.Polyglot.NoSql.Delegator.Proxy;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddMemoryCache();

        services
                .AddScoped<IServiceLogic, ServiceLogic>()
                .AddScoped<IValidator, Validator>()
                .AddScoped<ITranslate, Translate>()
                .AddTransient<ISchema, Schema>()
                .AddTransient<ICache, Cache>()
                .AddTransient<IInterpreter, Interpreter>()
                .AddTransient<ICommandEvent, CommandEvent>()
                .AddTransient<IProxy, Proxy>()
                .AddTransient<IRedisRepo, RedisRepo>()
                .AddTransient<ICassandraRepo, CassandraRepo>()
                .AddTransient<IMongoDbRepo, MongoDbRepo>()
                .AddTransient<INeo4jRepo, Neo4jRepo>()
                .AddSingleton<IRedisBridge, RedisBridge>()
                .AddSingleton<INeo4jBridge, Neo4jBridge>()
                .AddSingleton<ICassandraBridge, CassandraBridge>()
                .AddSingleton<IMongoDBBridge, MongoDbBridge>();
    })
    .Build();


ExemplifyScoping(host.Services, "Scope 1");

await host.RunAsync();

static void ExemplifyScoping(IServiceProvider services, string scope)
{
    using IServiceScope serviceScope = services.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    ServiceLogic logic = (ServiceLogic)provider.GetRequiredService<IServiceLogic>();

    Console.WriteLine("Loading Data...");

    logic.DataLoad();

    Console.WriteLine("Data Load Completed...");

    //var input = @" FETCH { name,avg(property), tester }
    //               DATA_MODEL { student }
    //               RESTRICT_TO { 1 }
    //               TARGET { redis }";

    //var input = @"FETCH { title, name, SUM(marks) }
    //              DATA_MODEL { student, transcript}
    //              LINK_ON { id = student_id }
    //              FILTER_ON {idnumber = '62408306136' AND marks > 50}
    //              GROUP_BY { title, name}
    //              RESTRICT_TO { 10 }
    //              TARGET {  mongodb }";
    //// neo4j, redis, cassandra,
    //Console.WriteLine("Executing query : " + input);

    //logic.Query(input);

    Console.WriteLine("Done...");
    Console.WriteLine();
}

