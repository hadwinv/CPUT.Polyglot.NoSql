using CPUT.Polyglot.NoSql.Common.Reporting;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Models;
using CPUT.Polyglot.NoSql.Models.Views.Bindings;
using CsvHelper;
using Microsoft.Win32;
using Newtonsoft.Json;
using Superpower;
using Superpower.Model;
using System.Formats.Asn1;
using System.Globalization;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;
using Result = CPUT.Polyglot.NoSql.Models.Result;

namespace CPUT.Polyglot.NoSql.Console
{
    public class ExecutionBuilder
    {
        private IServiceLogic _serviceLogic;

        private Dictionary<int, RegisteryModel> _registeries { get; set; }

        public ExecutionBuilder(IServiceLogic serviceLogic)
        {
            _serviceLogic = serviceLogic;
            _registeries = new Dictionary<int, RegisteryModel>();
        }

        public void Run()
        {
            try
            {
                var applyGloabalTags = false;

                var @base = @"C:\metrics\";

                var subdirectory = string.Empty;

                if (!applyGloabalTags)
                    subdirectory = @"data\";
                else
                    subdirectory = @"data\cumulative";

                if (@base != @"C:\")
                    foreach (System.IO.FileInfo file in new System.IO.DirectoryInfo(@base).GetFiles()) file.Delete();

                if ((@base + subdirectory) != @"C:\")
                    foreach (System.IO.FileInfo file in new System.IO.DirectoryInfo(@base + subdirectory).GetFiles()) file.Delete();

                var directory = @base + subdirectory;

                var busy = false;

                var resetMetrics = false;
                var writeResultTofile = true;
                

                foreach (var registry in _registeries.Values.Where(x => x.Active).OrderBy(x => x.No))
                {
                    var database = Common.Helpers.Utils.Database.NONE;

                    System.Console.WriteLine(string.Format("Run Test - Target: {0}, Number : {1}, Command : {2}, Descripton : {3}", registry.Target, registry.No, registry.Command, registry.Description));

                    if(applyGloabalTags)
                        MetricsRegistry.Tag("cumulative");
                    else
                        MetricsRegistry.Tag(string.Format("test no : {0}", registry.No));

                    var results = _serviceLogic.Query(registry.Script);

                    if (writeResultTofile)
                    {
                        //write  native query
                        var gquery = string.Format(directory + "Test {0} - {1} unified query.txt", registry.No, registry.Command.ToLower());

                        //write file
                        File.WriteAllText(gquery, registry.Script);
                    }

                    if (results != null && results.Count() > 0)
                    {
                        foreach (var result in results)
                        {
                            System.Console.WriteLine(string.Format("Completed Test - Target: {0}, Number : {1}, Command : {2}, Status: {3}, Message : {4}", registry.Target, registry.No, registry.Command, result.Status, result.Message));

                            if(writeResultTofile)
                            {
                                //write  native query
                                var nquery = string.Format(directory + "Test {0} - {1} native {2} query.txt", registry.No, registry.Command.ToLower(), Enum.GetName(typeof(Database), result.Source).ToLower());

                                //write file
                                File.WriteAllText(nquery, result.Executable);

                                var output = string.Format(directory + "Test {0} - {1} results {2}.csv", registry.No, registry.Command.ToLower(), Enum.GetName(typeof(Database), result.Source).ToLower());

                                using var writer = new StreamWriter(output);
                                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                                if (result.Data != null && result.Data.Count > 0)
                                {
                                    csv.WriteHeader<ResultsModel>();
                                    csv.NextRecord();
                                    foreach (var data in result.Data)
                                    {
                                        csv.WriteRecord(data);
                                        csv.NextRecord();
                                    }
                                }
                                else
                                {
                                    csv.WriteRecord(result);
                                    csv.NextRecord();
                                }
                              
                            }
                            
                            database = result.Source;
                        }
                    }
                    else
                        System.Console.WriteLine(string.Format("Completed Test - Target: {0}, Number : {1}, Status: No Result", registry.Target, registry.No));

                    if(resetMetrics)
                        MetricsRegistry.Reset();
                }

                System.Console.WriteLine("Test Run Finished");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Run error");
            }
        }

        public void Setup()
        {
            try
            {
                //generate data
                //_serviceLogic.GenerateData();

                //System.Console.WriteLine("Data Generated...");

                //load data
                _serviceLogic.DataLoad();

                System.Console.WriteLine("Data Loaded...");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Setup errors...");
            }
        }

        public void Create()
        {
            var testno = 0;
            try
            {
                //#region "Single Targets"

                ////redis test cases
                //CreateRedisOnly(ref testno);

                ////cassandra test cases
                //CreateCassandraOnly(ref testno);

                ////mongodb test cases
                //CreateMongoDBOnly(ref testno);


                ////neo4j test cases
                //CreateNeo4jDBOnly(ref testno);

                //#endregion

                #region "Multiple Targets"

                CreateMoreThanOneTarget(ref testno);

                #endregion

                //CreateSyntaxAndSemanticeQueryError(ref testno);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Create test scenario errors...");
            }
        }

        private void CreateRedisOnly(ref int testno)
        {
            //get all data from database 1
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get all data from database",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          TARGET {   redis }",
                Active = true
            });

            //get data using filter based on key 2
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using filter based on key",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          FILTER_ON {s.idnumber = '32502601866'}
                          TARGET {   redis }",
                Active = true
            });

            //get data using filter based on non-key 3
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using filter based on non-key",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          FILTER_ON {s.gender = 'F'}
                          TARGET {   redis }",
                Active = true
            });

            //get data applying multiple filters 4
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data applying multiple filters",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '11807003413' AND s.gender = 'F'}
                          TARGET {   redis }",
                Active = true
            });

            //modify existing data 5
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify existing data",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { gender = 'M'}
                           FILTER_ON { idnumber = '34502402028||'}
                           TARGET {   redis }",
                Active = true
            });

            //modifies existing data where the target field is deemed complex 6
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modifies existing data where the target field is deemed complex",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { register.username = 'newuser', password = 'newpassword'}
                           FILTER_ON { idnumber = '47803702771'}
                           TARGET {   redis }",
                Active = true
            });

            //insert data including with key 7
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Add",
                Description = "insert data including key",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"ADD { student }
                           PROPERTIES { idnumber = '" + new Random().Next(100000000, 999999999).ToString() + @"', name = 'Chuck T', surname = 'Tester'}
                           TARGET {   redis }",
                Active = true
            });

            //insert data to excluding non-key 8
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Add",
                Description = "insert data excluding non-key",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"ADD { student }
                           PROPERTIES { name = 'Chuck T', surname = 'Tester'}
                           TARGET {   redis }",
                Active = true
            });
        }

        private void CreateCassandraOnly(ref int testno)
        {
            //get all data from database 9
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get all data from database",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date, s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          TARGET { cassandra }",
                Active = true
            });

            //get data using filter on clustered key 10
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using filter on clustered key",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          FILTER_ON {s.identifier = '200'}
                          TARGET {  cassandra }",
                Active = true,
            });

            //get data using more than one filter with AND clause specified 11
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using more than one filter with AND clause specified",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '71100307130' AND s.gender = 'M'}
                          TARGET { cassandra }",
                Active = true
            });

            //get data using more than one filter with OR clause specified 12
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using more than one filter with OR clause specified",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '71100307130' OR s.gender = 'M'}
                          TARGET { cassandra }",
                Active = true
            });

            //get x amount of records 13
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get x amount of records",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          RESTRICT_TO { 10 }
                          TARGET { cassandra }",
                Active = true
            });

            //sort data retrieval on non-clustered columns 14
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "sort data retrieval non-clustered columns",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '71100307130' OR s.gender = 'M'}
                          ORDER_BY { s.name} 
                          TARGET { cassandra }",
                Active = true
            });

            //sort data retrieval on clustered column 15
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "retrieves data with filter on partial clustered key and sort by non-clustered key",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.identifier, s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '71100307130' OR s.gender = 'M'}
                          ORDER_BY { s.identifier} 
                          TARGET { cassandra }",
                Active = true
            });

            //get data from source with filter on index key 16
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "retrieves data from source with filter on index key",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.gender = 'F'}
                          TARGET { cassandra }",
                Active = true
            });

            //get data from source with filter on non-index key 17
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "retrieves data from source with filter on non-index key",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.name = 'Matthew'}
                          TARGET { cassandra }",
                Active = true
            });

            //aggregate data using SUM function 18
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using SUM function",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NSUM(s.transcript.result) }
                              DATA_MODEL { student AS s }
                              FILTER_ON { s.idnumber = '77607500615'}
                              TARGET { cassandra }",
                Active = true
            });

            //aggregate data using AVG function 19
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using AVG function",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NAVG(s.transcript.result) }
                              DATA_MODEL { student AS s }
                              FILTER_ON { s.idnumber = '77607500615'}
                              TARGET { cassandra }",
                Active = true
            });

            //aggregate data using COUNT function 20
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using COUNT function",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NCOUNT(s.transcript.result) }
                              DATA_MODEL { student AS s }
                              FILTER_ON { s.idnumber = '77607500615'}
                              TARGET { cassandra }",
                Active = true
            });

            //aggregate data using MIN function  21
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using MIN function ",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NMIN(s.transcript.result) }
                              DATA_MODEL { student AS s }
                              FILTER_ON { s.idnumber = '77607500615'}
                              TARGET { cassandra }",
                Active = true
            });

            //retrieves data from source with MAX 22
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using MAX function ",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NMAX(s.transcript.result) }
                              DATA_MODEL { student AS s }
                              FILTER_ON { s.idnumber = '77607500615'}
                              TARGET { cassandra }",
                Active = true
            });

            //modify field(s)  with filter on clustered index 23
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify field(s)  with filter on clustered index",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { name = 'Test 1', surname = 'Test 2', initial = 'TT'}
                           FILTER_ON { identifier = '5' }
                           TARGET { cassandra }",
                Active = true
            });

            //modify field(s) with filter on indexed field 24
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify field(s) with filter on indexed field",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { name = 'Micheal', surname = 'Corleone', initial = 'M'}
                           FILTER_ON { idnumber = '65500804135'}
                           TARGET { cassandra }",
                Active = true
            });


            //modify field(s) with filter on non-index 25
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify field(s) with filter on index",
                Target = "Cassandra",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { name = 'John', surname = 'Doe', initial = 'JD'}
                           FILTER_ON { name =  'Micheal'}
                           TARGET { cassandra }",
                Active = true
            });

            //add field(s) with clustered index provided 26
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "add field(s) with full clustered index provided",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"ADD { student }
                           PROPERTIES { identifier = '" + new Random().Next(100000000, 999999999).ToString() + @"', 
                                        idnumber = '876765564431', 
                                        title = 'Miss', 
                                        name = 'Lauren', 
                                        surname = 'Cole', 
                                        register.studentno = '" + new Random().Next(1000000, 9999999).ToString() + @"' }
                           TARGET {   cassandra }",
                Active = true
            });

            //add field(s) with no clustered index provided 27
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "ADD",
                Description = "add field(s) with full clustered index provided",
                Target = "Redis",
                ExecutionTimes = 1,
                Script = @"ADD { student }
                           PROPERTIES { idnumber = '" + new Random().Next(1000000, 9999999).ToString() + @"', 
                                        title = 'Miss', 
                                        name = 'Lauren', 
                                        surname = 'Cole', 
                                        register.studentno = '9599999' }
                           TARGET {   cassandra }",
                Active = true
            });

        }

        private void CreateMongoDBOnly(ref int testno)
        {
            //get all data from database 28
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get all data from database",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          TARGET { mongodb }",
                Active = true
            });

            //get data using single filter 29
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using single filter",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          FILTER_ON {s.idnumber = '00503100763'}
                          TARGET {  mongodb }",
                Active = true,
            });

            //get data using more than one filter with an AND specified 30
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using more than one filter with an AND specified",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '51701205088' AND s.gender = 'M'}
                          TARGET { mongodb }",
                Active = true
            });

            //get x amount of records 31
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get x amount of records",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          RESTRICT_TO { 10 }
                          TARGET { mongodb }",
                Active = true
            });

            //get and sort data by field 32
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get and sort data by field",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          ORDER_BY { s.name} 
                          TARGET { mongodb }",
                Active = true
            });

            //get data using fileter and sort by field 33
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using filter and sort by field",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '58308108421'}
                          ORDER_BY { s.name} 
                          TARGET { mongodb }",
                Active = true
            });

            //get data using multiple filters with AND specified, sorted by field 34
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "retrieves data with multiple filters using AND filter on and sort by column",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '78702504377' AND  s.register.studentno = '779529903'}
                          ORDER_BY { s.name} 
                          TARGET { mongodb }",
                Active = true
            });

            //retrieves data with multiple filters using OR filter on and sorted by column 35
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "retrieves data with multiple OR in filters and sorted by column",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '24106105288' OR s.idnumber = '88404705416'}
                          ORDER_BY { s.name} 
                          TARGET { mongodb }",
                Active = true
            });

            //get data with OR/AND in filters and sort by column 36
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data with OR/AND in filters and sort by column",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '24106105288' OR s.idnumber = '88404705416' AND s.gender = 'F' }
                          ORDER_BY { s.name} 
                          TARGET { mongodb }",
                Active = true
            });

            //aggregate data using SUM function 37
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using SUM function",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NSUM(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '58602700606'}
                          TARGET { mongodb }",
                Active = true
            });

            //aggregate data using AVG function 38
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using AVG function",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NAVG(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '58602700606'}
                          TARGET { mongodb }",
                Active = true
            });

            //aggregate data using COUNT function 39
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using COUNT function",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NCOUNT(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '58602700606'}
                          TARGET { mongodb }",
                Active = true
            });

            //aggregate data using MIN function 40
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using MIN function ",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NMIN(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '58602700606'}
                          TARGET { mongodb }",
                Active = true
            });

            //retrieves data from source with MAX 41
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using MAX function ",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NMAX(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '58602700606'}
                          TARGET { mongodb }",
                Active = true
            });

            //modify field(s) with single filter 42
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify field(s) with single filter",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { name = 'Jane', surname = 'Doe', initial = 'JD'}
                           FILTER_ON { idnumber = '83604407222'}
                           TARGET { mongodb }",
                Active = true
            });

            //modify field(s) with muliple filters 43
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify field(s) with muliple filters",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { name = 'Jane-Anne', surname = 'Jenkins', initial = 'JA'}
                           FILTER_ON { idnumber = '57508002711' AND register.studentno = '391050029'}
                           TARGET { mongodb }",
                Active = true
            });

            //add field(s) 44
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Add",
                Description = "add field(s)",
                Target = "MongoDB",
                ExecutionTimes = 1,
                Script = @"ADD { student }
                           PROPERTIES { 
                                        idnumber = '" + new Random().Next(1000000, 9999999).ToString() + @"', 
                                        title = 'Miss', 
                                        name = 'Lauren', 
                                        surname = 'Cole', 
                                        register.studentno = '" + new Random().Next(10000, 99999).ToString() + @"' }
                           TARGET {   mongodb }",
                Active = true
            });

        }

        private void CreateNeo4jDBOnly(ref int testno)
        {
            //get all data from database 45
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get all data from database",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data without filter referencing 6 out of 7 the nodes 46
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data without filter referencing 6 out of 7 the nodes",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date }
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data without filter referencing 5 out of 7 the nodes 47
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data without filter referencing 5 out of 7 the nodes",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date }
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data without filter referencing 5 out of 7 the nodes with optional match 48
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data without filter referencing 5 out of 7 the nodes with optional match",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date }
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data without filter referencing 4 out of 7 the nodes with optional match 49
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data without filter referencing 4 out of 7 the nodes with optional match",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, 
                                  s.register.course.code, s.register.course.name, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date }
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data without filter referencing 3 out of 7 the nodes 50
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data without filter referencing 3 out of 7 the nodes",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, 
                                  s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date }
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data without filter referencing 3 out of 7 the nodes, part 1 51
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "retrieves data from source without filter referencing 2 out of 7 the nodes, part 1",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile}
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data without filter referencing 3 out of 7 the nodes, part 2 52
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data without filter referencing 3 out of 7 the nodes, part 2",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,
                                  s.contact.email, s.contact.mobile}
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data without filter referencing 1 out of 7 the nodes 53
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data without filter referencing 1 out of 7 the nodes",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,
                                  s.contact.email, s.contact.mobile}
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get data using single filter 54
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data using single filter",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,
                                  s.contact.email, s.contact.mobile}
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '72800706875'}
                          TARGET { neo4j }",
                Active = true
            });

            //get data with AND specified in filter  55
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data with AND specified in filter",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,
                                  s.contact.email, s.contact.mobile}
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '67101803610' AND s.gender = 'F'}
                          TARGET { neo4j }",
                Active = true
            });

            //get data with OR specified in filter 56
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data with OR specified in filter",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,
                                  s.contact.email, s.contact.mobile}
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.idnumber = '67101803610' OR s.gender = 'M'}
                          RESTRICT_TO { 100 }
                          TARGET { neo4j }",
                Active = true
            });

            //get and sort data using filter 57
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get and sort data using filter",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,
                                  s.contact.email, s.contact.mobile}
                          DATA_MODEL { student AS s}
                          FILTER_ON { s.gender = 'M'}
                          RESTRICT_TO { 100 }
                          ORDER_BY { s.surname, s.name} 
                          TARGET { neo4j }",
                Active = true
            });

            //aggregate data using SUM function 58
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using SUM function",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NSUM(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '88404705416'}
                          TARGET { neo4j }",
                Active = true
            });

            //aggregate data using AVG function 59
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using AVG function",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NAVG(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '88404705416'}
                          TARGET { neo4j }",
                Active = true
            });

            //aggregate data using COUNT function 60
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using COUNT function",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NCOUNT(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '88404705416'}
                          TARGET { neo4j }",
                Active = true
            });

            //aggregate data using MIN function 61
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using MIN function ",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NMIN(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '88404705416'}
                          TARGET { neo4j }",
                Active = true
            });

            //retrieves data from source with MAX 62
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "aggregate data using MAX function ",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NMAX(s.transcript.result) }
                          DATA_MODEL { student AS s }
                          FILTER_ON { s.idnumber = '88404705416'}
                          TARGET { neo4j }",
                Active = true
            });

            //modify field(s) with single filter 63
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify field(s) with single filter",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { name = 'Jane', surname = 'Doe', initial = 'JD'}
                           FILTER_ON { idnumber = '83604407222'}
                           TARGET { neo4j }",
                Active = true
            });

            //modify field(s) with muliple filters 64
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify field(s) with muliple filters",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           PROPERTIES { name = 'Jane-Anne', surname = 'Jenkins', initial = 'JA'}
                           FILTER_ON { idnumber = '57508002711' AND register.studentno = '391050029'}
                           TARGET { neo4j }",
                Active = true
            });

            //add field(s)  65
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Add",
                Description = "add field(s) ",
                Target = "Neo4j",
                ExecutionTimes = 1,
                Script = @"ADD { student }
                           PROPERTIES { 
                                        idnumber = '" + new Random().Next(1000000, 9999999).ToString() + @"', 
                                        title = 'Miss', 
                                        name = 'Lauren', 
                                        surname = 'Cole'
                                        }
                           TARGET {   neo4j }",
                Active = true
            });
        }

        private void CreateMoreThanOneTarget(ref int testno)
        {
            //get data from all target databases source without filters restrict to 1000 records 66
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "get data from all target databases source without filters restrict to 1000 records",
                Target = "ALL",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          DATA_MODEL { student AS s}
                          RESTRICT_TO { 1000 }
                          TARGET { redis, cassandra, mongodb, neo4j }",
                Active = true
            });

            ////get data from all target databases source with single filter 67
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data from all target databases source with single filter",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
            //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
            //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
            //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
            //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
            //                      s.register.date }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.idnumber = '67101803610'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////get data from all target databases source with multiple filters using AND 68
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data from all target databases source with multiple filters using AND",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
            //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
            //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
            //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
            //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
            //                      s.register.date }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.idnumber = '67101803610' AND s.gender = 'F'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////get data from all target databases source with multiple filters using OR 69
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data from all target databases source with multiple filters using AND",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
            //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
            //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
            //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
            //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
            //                      s.register.date }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.idnumber = '67101803610' OR s.gender = 'M'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //}); 

            ////get data from all target databases source with multiple filters using OR 70
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data from all target databases source with multiple filters using AND",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
            //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
            //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
            //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
            //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
            //                      s.register.date }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.idnumber = '67101803610' OR s.gender = 'M' AND s.register.studentno = '979883209'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////get data from all target databases source with sorting specified 71
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data from all target databases source with sorting specified",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
            //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
            //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
            //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
            //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
            //                      s.register.date }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.idnumber = '67101803610' OR s.gender = 'M' AND s.register.studentno = '979883209'}
            //              ORDER_BY { s.surname, s.idnumber} 
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////aggregate data using SUM function 72
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "aggregate data using SUM function",
            //    Target = "Neo4j",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NSUM(s.transcript.result) }
            //              DATA_MODEL { student AS s }
            //              FILTER_ON { s.idnumber = '21708702176'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////aggregate data using AVG function 73
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "aggregate data using AVG function",
            //    Target = "Neo4j",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NAVG(s.transcript.result) }
            //              DATA_MODEL { student AS s }
            //              FILTER_ON { s.idnumber = '21708702176'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////aggregate data using COUNT function 74
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "aggregate data using COUNT function",
            //    Target = "Neo4j",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NCOUNT(s.transcript.result) }
            //              DATA_MODEL { student AS s }
            //              FILTER_ON { s.idnumber = '21708702176'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////aggregate data using MIN function 75
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "aggregate data using MIN function ",
            //    Target = "Neo4j",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NMIN(s.transcript.result) }
            //              DATA_MODEL { student AS s }
            //              FILTER_ON { s.idnumber = '21708702176'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////retrieves data from source with MAX 76
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "aggregate data using MAX function ",
            //    Target = "Neo4j",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NMAX(s.transcript.result) }
            //              DATA_MODEL { student AS s }
            //              FILTER_ON { s.idnumber = '21708702176'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////get students where results are more than 50 77
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data from all target databases source without filters restrict to 1000 records",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname,
            //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.transcript.result > 50}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////get students where results are less than 50 78
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data from all target databases source without filters restrict to 1000 records",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname,
            //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.transcript.result < 50}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});


            ////get data where results between 20 and 70 79
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data where results between 20 and 70",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname,
            //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.transcript.result >= 20 AND s.transcript.result <= 70}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////get data where results symbol equals 'A' or 'B' where filter is in not in the selection 80
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data where results symbol equals 'A' OR 'B' where filter is in not in the selection",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname,
            //                       s.register.faculty.code, s.register.faculty.name, 
            //                       s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name  }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.transcript.symbol = 'A' OR s.transcript.symbol = 'B'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////get data where results symbol equals 'A' or 'B' where filter is in in the selection 81
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Fetch",
            //    Description = "get data where results symbol equals 'A' or 'B' where filter is in in the selection",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname,
            //                       s.register.faculty.code, s.register.faculty.name, 
            //                       s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, s.transcript.symbol  }
            //              DATA_MODEL { student AS s}
            //              FILTER_ON { s.transcript.symbol = 'A' OR s.transcript.symbol = 'B'}
            //              TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////modify field(s) with single filter 82
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Modify",
            //    Description = "modify field(s) with single filter",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"MODIFY { student }
            //               PROPERTIES { name = 'Mary', surname = 'Poppins', initial = 'MP'}
            //               FILTER_ON { idnumber = '85208201670'}
            //               TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////modify field(s) with muliple filters where some properties are not support by target dabases 83
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Modify",
            //    Description = "modify field(s) with muliple filters where some properties are not support by target dabases",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"MODIFY { student }
            //               PROPERTIES { name = 'Clark', 
            //                            surname = 'Kent', 
            //                            initial = 'Mel',
            //                            title = 'MR',
            //                            preferredname = 'Superman'
            //                          }
            //               FILTER_ON { idnumber = '75602501070'}
            //               TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////modify field(s) where filter is not support by target databases 84
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Modify",
            //    Description = "modify field(s) where filter is not support by target databases",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"MODIFY { student }
            //               PROPERTIES { name = 'Clark', 
            //                            surname = 'Kent', 
            //                            initial = 'Mel',
            //                            gender = 'M',
            //                            title = 'MR',
            //                            preferredname = 'Superman'
            //                          }
            //               FILTER_ON { identifier = '10000' }
            //               TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////modify field(s) on where targeted field i a date 85
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Modify",
            //    Description = "modify field(s) where filter is not support by target databases",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"MODIFY { student }
            //               PROPERTIES { dateofbirth ='1970/10/13'}
            //               FILTER_ON { identifier = '10000' }
            //               TARGET { redis, cassandra, mongodb, neo4j }",
            //    Active = true
            //});

            ////add field(s) 86
            //testno++;
            //_registeries.Add(testno, new RegisteryModel
            //{
            //    No = testno,
            //    Command = "Add",
            //    Description = "add field(s)",
            //    Target = "ALL",
            //    ExecutionTimes = 1,
            //    Script = @"ADD { student }
            //               PROPERTIES { 
            //                            identifier='" + new Random().Next(1000000, 9999999).ToString() + @"', 
            //                            idnumber = '" + new Random().Next(100000000, 999999900).ToString() + @"', 
            //                            surname = 'Banner', 
            //                            name = 'Bruce', 
            //                            initial = 'BB',
            //                            gender = 'M',
            //                            title = 'Mr',
            //                            dateofbirth ='1970/10/13'
            //                            preferredname = 'Hulk'
            //                            }
            //               TARGET { redis, cassandra, mongodb, neo4j  }",
            //    Active = true
            //});
        }

        private void CreateSyntaxAndSemanticeQueryError(ref int testno)
        {
            //Fetch data where restriction is placed before data model 87
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "Fetch data where restriction is placed before data model",
                Target = "ALL",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                  s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                  s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                  s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                  s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                  s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                          RESTRICT_TO { 1000 }
                          DATA_MODEL { student AS s}
                          TARGET { redis, cassandra, mongodb, neo4j }",
                Active = true
            });

            //Fetch query contains a dangling comma in query 88
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "Fetch query contains a dangling comma in query",
                Target = "ALL",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, }
                          DATA_MODEL { student AS s}
                          TARGET { redis, cassandra, mongodb, neo4j }",
                Active = true
            });

            //Fetch query containing a property not defined in the unified schema  89
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Fetch",
                Description = "Fetch query contains a dangling comma in query",
                Target = "ALL",
                ExecutionTimes = 1,
                Script = @"FETCH { s.title,s.idnumber,s.newproperty1,s.newproperty2 }
                          DATA_MODEL { student AS s}
                          TARGET { redis, cassandra, mongodb, neo4j }",
                Active = true
            });

            //modify field(s) on where filter is placed before properties 90
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Modify",
                Description = "modify field(s) on where filter is placed before properties",
                Target = "ALL",
                ExecutionTimes = 1,
                Script = @"MODIFY { student }
                           FILTER_ON { identifier = '10000' }
                           PROPERTIES { name = 'Tony'}
                           TARGET { redis, cassandra, mongodb, neo4j }",
                Active = true
            });

            //add incorrectly formated query 90
            testno++;
            _registeries.Add(testno, new RegisteryModel
            {
                No = testno,
                Command = "Add",
                Description = "add incorrectly formated query",
                Target = "ALL",
                ExecutionTimes = 1,
                Script = @"
                           PROPERTIES { 
                                        identifier='" + new Random().Next(1000000, 9999999).ToString() + @"', 
                                        idnumber = '" + new Random().Next(100000000, 999999900).ToString() + @"', 
                                        surname = 'Banner', 
                                        name = 'Bruce', 
                                        initial = 'BB',
                                        gender = 'M',
                                        title = 'Mr',
                                        preferredname = 'Hulk'
                                        }
                           ADD { student }
                           TARGET { redis, cassandra, mongodb, neo4j  }",
                Active = true
            });

        }
    }
}
