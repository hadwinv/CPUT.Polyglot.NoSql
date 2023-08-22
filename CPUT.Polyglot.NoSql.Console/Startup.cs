using CPUT.Polyglot.NoSql.DataStores.Repos.KeyValue;
using CPUT.Polyglot.NoSql.Interface.Repos;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Translator;
using CPUT.Polyglot.NoSql.Logic.Core.Events;
using CPUT.Polyglot.NoSql.Logic;
using CPUT.Polyglot.NoSql.Logic.Core;
using CPUT.Polyglot.NoSql.Translator;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Mapper;
using CPUT.Polyglot.NoSql.Translator.Events;
using CPUT.Polyglot.NoSql.DataStores.Repos.Columnar;
using CPUT.Polyglot.NoSql.DataStores.Repos.Document;
using CPUT.Polyglot.NoSql.Interface;
using CPUT.Polyglot.NoSql.Common;
using CPUT.Polyglot.NoSql.DataStores.Repos.Graph;
using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;
using CPUT.Polyglot.NoSql.DataStores.Repos._data;
using CPUT.Polyglot.NoSql.Delegator;
using CPUT.Polyglot.NoSql.Delegator.Adaptors;
using App.Metrics;
using CPUT.Polyglot.NoSql.Console.Middleware;

namespace CPUT.Polyglot.NoSql.Console
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

       

        public void ConfigureServices(IServiceCollection services)
        {
            //configure your services here
            services
                .AddMemoryCache()
                .AddScoped<IServiceLogic, ServiceLogic>()
                .AddScoped<IValidator, Validator>()
                .AddScoped<ITranslate, Translate>()
                .AddScoped<ISchema, Schema>()
                .AddScoped<IInterpreter, Interpreter>()
                .AddScoped<ICommandEvent, CommandEvent>()
                .AddScoped<IRedisRepo, RedisRepo>()
                .AddScoped<ICassandraRepo, CassandraRepo>()
                .AddScoped<IMongoDbRepo, MongoDbRepo>()
                .AddScoped<INeo4jRepo, Neo4jRepo>()
                .AddTransient<ICache, Cache>()
                .AddScoped<IExecutor, Executor>()
                .AddScoped<IRedisBridge, RedisBridge>()
                .AddScoped<INeo4jBridge, Neo4jBridge>()
                .AddScoped<ICassandraBridge, CassandraBridge>()
                .AddScoped<IMongoDBBridge, MongoDbBridge>()
                .AddScoped<IMockData, MockData>();

            //application metrics
            AppMetricsExtension.AddMetricsExtension(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            app.UseMetrics();
            //app.UseMetricsReporting((Microsoft.AspNetCore.Hosting.IApplicationLifetime)lifetime);
        } 
    }
}
