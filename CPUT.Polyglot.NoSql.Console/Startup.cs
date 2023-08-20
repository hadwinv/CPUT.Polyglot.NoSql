using CPUT.Polyglot.NoSql.DataStores.Repos.KeyValue;
using CPUT.Polyglot.NoSql.DataStores;
using CPUT.Polyglot.NoSql.Interface.Repos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using App.Metrics.Filtering;
using App.Metrics;
using App.Metrics.Formatters.InfluxDB;
using App.Metrics.Filters;

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
            //set up metrics
            //var filter = new MetricsFilter().WhereType(MetricType.Timer);

            var metrics = new MetricsBuilder()
                .Report.ToInfluxDb(
                    options =>
                    {
                        options.InfluxDb.BaseUri = new Uri("http://127.0.0.1:8086");
                        options.InfluxDb.Database = "hadwin";
                        options.InfluxDb.Consistenency = "consistency";
                        options.InfluxDb.UserName = "admin";
                        options.InfluxDb.Password = "adminadmin";
                        options.InfluxDb.RetentionPolicy = "rp";
                        options.InfluxDb.CreateDataBaseIfNotExists = true;
                        options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                        options.HttpPolicy.FailuresBeforeBackoff = 5;
                        options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                        options.MetricsOutputFormatter = new MetricsInfluxDbLineProtocolOutputFormatter();
                        //options.Filter = filter;
                        options.FlushInterval = TimeSpan.FromSeconds(20);
                    })
                .Report.ToTextFile(@"C:\metrics\metrics.txt")
                .Report.ToConsole()
            .Build();


            //configure your services here
            services
                .AddMemoryCache()
                .AddMetrics(metrics)
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
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMetrics();
        } 
    }
}
