using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Mapper.ViewMap;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models._data.prep.MongoDb;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Pipelines.Sockets.Unofficial.Arenas;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Document
{
    public class MongoDbRepo : IMongoDbRepo
    {
        private readonly IMongoDBBridge _connector;

        public MongoDbRepo(IMongoDBBridge connector)
        {
            _connector = connector;
        }

        public Models.Result Execute(QueryDirective query)
        {
            Models.Result result = null;

            try
            {
                if (query.Executable != null)
                {
                    dynamic data = null;

                    //connect to database
                    var db = _connector.Connect();

                    var executable = BsonSerializer.Deserialize<BsonDocument>(query.Executable);

                    if (executable.ElementAt(0).Name == "aggregate" || executable.ElementAt(0).Name == "find")
                    {
                        IAsyncCursor<BsonDocument> response = null;

                        var collection = db.GetCollection<BsonDocument>(executable.ElementAt(0).Value.ToString());

                        if (executable.ElementAt(0).Name == "aggregate")
                        {
                            var pipeline = BsonSerializer.Deserialize<BsonDocument[]>(executable.ElementAt(1).Value.ToString());

                            response = collection.Aggregate<BsonDocument>(pipeline);
                        }
                        else if (executable.ElementAt(0).Name == "find")
                        {
                            var filter = new BsonDocument();

                            var options = new FindOptions<BsonDocument>()
                            {
                                Projection = executable.Contains("projection") ? BsonDocument.Parse(executable.GetElement("projection").Value.ToString()) : default,
                                Sort = executable.Contains("sort") ? BsonDocument.Parse(executable.GetElement("sort").Value.ToString()) : default,
                                Limit = executable.Contains("limit") ? int.Parse(executable.GetElement("limit").Value.ToString()) : default
                            };

                            if (executable.Contains("filter"))
                                filter = BsonDocument.Parse(executable.GetElement("filter").Value.ToString());

                            response = collection.FindAsync(filter, options).Result;
                        }
                        
                        //check if codex instructions were configured
                        if (query.Codex != null)
                            data = ModelBuilder.Create(query.Codex, response);

                        result = new Models.Result
                        {
                            Source = Common.Helpers.Utils.Database.MONGODB,
                            Data = data,
                            Status = "OK",
                            Message = string.Format("Query returned {0} record(s)", data.Count),
                            Success = true
                        };
                    }
                    else
                    {
                        var document = db.RunCommand<BsonDocument>(query.Executable);

                        if (document.Contains("ok"))
                        {
                            return new Models.Result
                            {
                                Source = Common.Helpers.Utils.Database.MONGODB,
                                Data = null,
                                Status = "OK",
                                Message = string.Format("{0} executed sucessfully", Enum.GetName(typeof(Command), query.Command).ToUpper()),
                                Success = true
                            };
                        }
                        else
                        {
                            return new Models.Result
                            {
                                Source = Common.Helpers.Utils.Database.MONGODB,
                                Data = null,
                                Status = "Failed",
                                Message = string.Format("{0} executed unsucessfully", Enum.GetName(typeof(Command), query.Command).ToUpper()),
                                Success = false
                            };
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");

                result = new Models.Result
                {
                    Source = Common.Helpers.Utils.Database.MONGODB,
                    Data = null,
                    Status = "Error",
                    Message = ex.Message,
                    Success = false
                };
            }

            return result;
        }
    }
}
