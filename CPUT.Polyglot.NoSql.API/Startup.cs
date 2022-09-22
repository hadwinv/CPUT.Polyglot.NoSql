using Cassandra;
using CPUT.Polyglot.NoSql.Adaptor.Connectors.Columnar;
using CPUT.Polyglot.NoSql.Adaptor.Connectors.Document;
using CPUT.Polyglot.NoSql.Adaptor.Connectors.Graph;
using CPUT.Polyglot.NoSql.Adaptor.Connectors.KeyValue;
using CPUT.Polyglot.NoSql.DataStores.Repos;
using CPUT.Polyglot.NoSql.DataStores.Repos.Columnar;
using CPUT.Polyglot.NoSql.DataStores.Repos.Document;
using CPUT.Polyglot.NoSql.DataStores.Repos.Graph;
using CPUT.Polyglot.NoSql.DataStores.Repos.KeyValue;
using CPUT.Polyglot.NoSql.Interface.Adaptors;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Middleware;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Logic;
using CPUT.Polyglot.NoSql.Middleware.Commands;
using CPUT.Polyglot.NoSql.Middleware.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Neo4j.Driver;
using StackExchange.Redis;

namespace CPUT.Polyglot.NoSql.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllers();

            // services
            services.AddTransient<IServiceLogic, ServiceLogic>();

            //commands
            services.AddTransient<IPolyCommand, PolyCommand>();
            
            //repos
            services.AddTransient<IRedisRepo, RedisRepo>();
            services.AddTransient<ICassandraRepo, CassandraRepo>();
            services.AddTransient<IMongoRepo, MongoRepo>();
            services.AddTransient<INeo4jRepo, Neo4jRepo>();

            //mock data
            services.AddTransient<IDataFactory, DataFactory>();
            services.AddTransient<IDataLoader, DataLoader>();

            //set database connections
            //redis
            services.AddSingleton<IRedisConnector, RedisConnector>();
            //neo4j
            services.AddSingleton<INeo4jConnector, Neo4jConnector>();
            //cassandra
            services.AddSingleton<ICassandraConnector, CassandraConnector>();
            //mongoDB
            services.AddSingleton<IMongoDBConnector, MongoDBConnector>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
           // app.UseMvc();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

           // app.UseMvc(
           //routes => {
           //    routes.MapRoute(
           //        name: "api",
           //        template: "api/{controller}/{action}/{id?}");


           //});
        }
    }
}
