using Cassandra;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Schema._data.prep;
using System;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Columnar
{
    public class CassandraRepo : ICassandraRepo
    {
        private ISession _session;

        public CassandraRepo(ISession session)
        {
            _session = session;
        }

        public void CreateColumnarDB(List<UDataset> dataset)
        {
            //UserModel user = null;
            try
            {
                _session.Execute("CREATE KEYSPACE uprofile WITH REPLICATION = { 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 };");

                ////clear all keys
                //_connector.GetServer("127.0.0.1:6379").FlushDatabase(1);

                //int count = 0;

                //foreach (var student in dataset[0].Students)
                //{
                //    if (count > 1000)
                //        break;

                //    user = new UserModel
                //    {
                //        identity_number = student.IdNumber,
                //        first_name = student.Name,
                //        last_name = student.Surname,
                //        preferred_name = student.Name,
                //        user_name = student.Name + student.Surname.Substring(0, 1),
                //        ip_address = student.Profile.IPAddress,
                //        device = student.Name,
                //        session_id = student.Profile.ProfileId,
                //        login_date = DateTime.Now.AddMinutes(-30),
                //        logout_date = DateTime.Now,
                //    };

                //    var jsonConvert = JsonConvert.SerializeObject(user);

                //    redis.StringSet(key: student.IdNumber, value: jsonConvert, expiry: new TimeSpan(0, 0, 1440, 0));

                //    count++;
                //}
            }
            // Capture any errors along with the query and data for traceability
            //catch (CassandraException ex)
            //{
            //    //Console.WriteLine($"{query} - {ex}");
            //    throw;
            //}
            catch (Exception ex)
            {
                //Console.WriteLine($"{query} - {ex}");
                throw;
            }
        }
    }
}
