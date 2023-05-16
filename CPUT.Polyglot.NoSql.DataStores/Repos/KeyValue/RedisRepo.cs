using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Native.Redis;
using CPUT.Polyglot.NoSql.Models.Translator;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.KeyValue
{
    public class RedisRepo : IRedisRepo
    {
        private IRedisBridge _connector;

        public RedisRepo(IRedisBridge connector)
        {
            _connector = connector;
           
        }

        public Models.Result Execute(Constructs construct)
        {
            RedisResult result;

            try
            {
                var redis = _connector.Connect();

                if(construct.Query != null)
                {
                    //var redisCmd = (RedisExecutor)construct.Query;

                    //result = redis.Execute(redisCmd.Key, redisCmd.Value);
                }
                
                int i = 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
            return null;
        }
        ////Create \update
        //var tests = redis.GetDatabase(1).Execute("SET", new []{ "createwithvalue", "update" } );

        #region Data Load

        public void Load(List<UDataset> dataset)
        {
            UserModel user = null;
            int count = 0;

            try
            {
                var redis = _connector.Connect();

                //clear all keys
                _connector.Flush();

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
                        user_name = student.Name + student.Surname.Substring(0, 1),
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
            catch (RedisException ex)
            {
                Console.WriteLine($"RedisException - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
                throw;
            }
            finally
            {
                if (_connector != null)
                    _connector.Disconnect();
            }
        }

        #endregion
    }
}
