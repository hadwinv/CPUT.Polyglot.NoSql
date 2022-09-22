using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Schema._data.prep;
using CPUT.Polyglot.NoSql.Schema.Local.Redis;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.KeyValue
{
    public class RedisRepo : IRedisRepo
    {
        private IConnectionMultiplexer _connector;
        private IDataLoader _dataLoader;

        public RedisRepo(IConnectionMultiplexer connector)
        {
            _connector = connector;
        }

        public void CreateKeyValueDB(List<UDataset> dataset)
        {
            UserModel user = null;
            try
            {
                var redis = _connector.GetDatabase(1);

                //clear all keys
                _connector.GetServer("127.0.0.1:6379").FlushDatabase(1);

                int count = 0;

                foreach (var student in dataset[0].Students)
                {
                    if (count > 1000)
                        break;
        
                    user = new UserModel
                    {
                        identity_number = student.IdNumber,
                        first_name = student.Name,
                        last_name = student.Surname,
                        preferred_name = student.Name,
                        user_name = student.Name + student.Surname.Substring(0,1),
                        ip_address = student.Profile.IPAddress,
                        device = student.Name,
                        session_id = Guid.NewGuid().ToString(),
                        login_date = DateTime.Now.AddMinutes(-30),
                        logout_date = DateTime.Now,
                    };

                    var jsonConvert = JsonConvert.SerializeObject(user);

                    redis.StringSet(key: student.IdNumber, value: jsonConvert, expiry: new TimeSpan(0, 0, 1440, 0));

                    count++;
                }
            }
            // Capture any errors along with the query and data for traceability
            catch (RedisException ex)
            {
                //Console.WriteLine($"{query} - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"{query} - {ex}");
                throw;
            }
        }
    }
}
