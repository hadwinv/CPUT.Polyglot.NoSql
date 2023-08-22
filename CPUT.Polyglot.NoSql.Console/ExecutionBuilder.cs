using Amazon.Auth.AccessControlPolicy;
using App.Metrics.Counter;
using App.Metrics;
using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Superpower;

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
                foreach (var registry in _registeries.Values.Where(x => x.Active).OrderBy(x => x.No))
                {
                    System.Console.WriteLine(string.Format("Run Test - Number : {0}, Descripton : {1}", registry.No, registry.Description));

                    var results = _serviceLogic.Query(registry.Script);

                    var database = Common.Helpers.Utils.Database.NONE;

                    foreach(var result in results)
                    {
                        System.Console.WriteLine(string.Format("Run Test - Status: {0}, Message : {1}", result.Status, result.Message));

                        //write to file
                        //foreach (var data in result.Data)
                        //    System.Console.WriteLine(JsonConvert.SerializeObject(data));

                        database = result.Source;
                    }
                    
                }
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
                _serviceLogic.GenerateData();

                //load data
                _serviceLogic.DataLoad();

                foreach (var registry in _registeries.Values.Where(x => x.Active).OrderBy(x => x.No))
                {
                    var result = _serviceLogic.Query(registry.Script);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Setup errors...");
            }
        }

        public void Create()
        {
            var testno = 1;
            try
            {
                #region "Fetch from supported targets"

                #region "Redis only"

                ////retrieves data from source without filter
                _registeries.Add(testno, new RegisteryModel
                {
                    No = testno,
                    Description = "Retrieves data from source without filter",
                    Target = "Redis",
                    ExecutionTimes = 1,
                    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                              DATA_MODEL { student AS s}
                              TARGET {   redis }",
                    Active = true
                });

                //retrieves data from source with filter on key
                testno++;
                _registeries.Add(testno, new RegisteryModel
                {
                    No = testno,
                    Description = "Retrieves data from source with filter on key",
                    Target = "Redis",
                    ExecutionTimes = 1,
                    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                              DATA_MODEL { student AS s}
                              FILTER_ON {s.idnumber = '35808404617'}
                              TARGET {   redis }",
                    Active = true
                });

                //retrieves data from source with filter not on key
                testno++;
                _registeries.Add(testno, new RegisteryModel
                {
                    No = testno,
                    Description = "Retrieves data from source with filter not on key",
                    Target = "Redis",
                    ExecutionTimes = 1,
                    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                              DATA_MODEL { student AS s}
                              FILTER_ON {s.gender = 'F'}
                              TARGET {   redis }",
                    Active = true
                });

                ////retrieves data from source with more than one filter on key AND non-key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "Retrieves data from source with more than one filter on key AND non-key",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '35808404617' AND s.gender = 'F'}
                //              TARGET {   redis }",
                //    Active = true
                //});

                ////retrieves data from source with more than one filter on key OR non-key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "Retrieves data from source with more than one filter on key OR non-key",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '16605107850' OR s.gender = 'M'}
                //              TARGET {   redis }",
                //    Active = true
                //});

                ////retrieves data from source with more than one filter on key OR non-key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "Retrieves data from source on more than one id numbers",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '45506503542' OR s.idnumber = '06808204055' }
                //              TARGET {   redis }",
                //    Active = true
                //});

                ////modifies primitive fields
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modifies primitive binding fields",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Test 1', surname = 'Test 2', initial = 'TT'}
                //               FILTER_ON { idnumber = '45506503542'}
                //               TARGET {   redis }",
                //    Active = true
                //});

                ////modifies complex binding data
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modifies complex binding data",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { register.username = 'newuser', password = 'newpassword'}
                //               FILTER_ON { idnumber = '06808204055'}
                //               TARGET {   redis }",
                //    Active = true
                //});

                ////add data with key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "add data with key",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"ADD { student }
                //               PROPERTIES { idnumber = '12345', name = 'Chuck T', surname = 'Tester'}
                //               TARGET {   redis }",
                //    Active = true
                //});

                ////add data with key without key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "add data with key without key",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"ADD { student }
                //               PROPERTIES { name = 'Chuck T', surname = 'Tester'}
                //               TARGET {   redis }",
                //    Active = true
                //});

                #endregion

                #region "Cassandra only"

                ////retrieves data from source without filter
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date, s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data from source with filter on partial clustered key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with filter on clustered key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON {s.idnumber = '04001104036'}
                //              TARGET {  cassandra }",
                //    Active = true,
                //});

                ////retrieves data from source with more than one filter using AND clause on partial clustered and non-clustered key)
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with more than one filter using AND clause on partial clustered and non-clustered key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '04001104036' AND s.gender = 'M'}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data x amount of records
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data x amount of records",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              RESTRICT_TO { 10 }
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////sort data retrieval non-clustered columns
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "sort data retrieval non-clustered columns",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              ORDER_BY { s.name} 
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data with filter on partial clustered key and sort by non-clustered key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data with filter on partial clustered key and sort by non-clustered key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '04001104036'}
                //              ORDER_BY { s.name} 
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data with filter on full clustered key and sort by non-clustered key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data with filter on full clustered key and sort by non-clustered key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.identifier = '11' AND s.idnumber = '04001104036' AND  s.register.studentno = '451782010'}
                //              ORDER_BY { s.name} 
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data with filter on full clustered key and sort by a clustered key??
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data with filter on full clustered key and sort by a clustered key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.identifier = '11' AND s.idnumber = '04001104036' AND  s.register.studentno = '451782010'}
                //              ORDER_BY { s.idnumber} 
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data from source with filter on index key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with filter on index key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.gender = 'F'}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data from source with filter on non-index key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with filter on non-index key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.name = 'Matthew'}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data from source with filter on non-index key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with filter on non-index key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.name = 'Matthew' OR s.name = 'Noemi' OR s.name = 'Gil' }
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data from source with filter (OR only) on partial clustered key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with filter (OR only) on partial clustered key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606' OR s.idnumber = '70803600564' OR s.idnumber = '83007003603' }
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data from source with filter(OR AND) on partial clustered key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with filter (OR AND) on partial clustered key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606' OR s.idnumber = '70803600564' AND s.idnumber = '83007003603' }
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data from source with filter (OR AND) on partial clustered key
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with filter (OR AND) on partial clustered key",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606' OR s.idnumber = '70803600564' AND s.idnumber = '83007003603' OR s.register.studentno = '397402990' OR s.register.studentno = '791349486' }
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////aggregate data using SUM function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using SUM function",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NSUM(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////aggregate data using AVG function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using AVG function",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NAVG(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////aggregate data using COUNT function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using COUNT function",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NCOUNT(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////aggregate data using MIN function 
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using MIN function ",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NMIN(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////retrieves data from source with MAX
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using MAX function ",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NMAX(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { cassandra }",
                //    Active = true
                //});

                ////modify field(s)  with filter on clustered index
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modify field(s)  with filter on clustered index",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Test 1', surname = 'Test 2', initial = 'TT'}
                //               FILTER_ON { identifier = '11' AND idnumber = '04001104036' AND register.studentno = '451782010'}
                //               TARGET { cassandra }",
                //    Active = true
                //});

                ////modify field(s) with filter on partial clustered index
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modify field(s) with filter on partial clustered index",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Micheal', surname = 'Corleone', initial = 'M'}
                //               FILTER_ON { idnumber = '04001104036' AND register.studentno = '451782010'}
                //               TARGET { cassandra }",
                //    Active = true
                //});

                ////modify field(s) with filter on index
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modify field(s) with filter on index",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Micheal', surname = 'Corleone', initial = 'M'}
                //               FILTER_ON { gender = 'M'}
                //               TARGET { cassandra }",
                //    Active = true
                //});

                ////modify field(s) with filter on non-index
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modify field(s) with filter on index",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Micheal Vito', surname = 'Corleone', initial = 'MV'}
                //               FILTER_ON { name =  'Micheal'}
                //               TARGET { cassandra }",
                //    Active = true
                //});

                ////add field(s) with full clustered index provided
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "add field(s) with full clustered index provided",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"ADD { student }
                //               PROPERTIES { identifier = '999999', 
                //                            idnumber = '87676556443', 
                //                            title = 'Miss', 
                //                            name = 'Lauren', 
                //                            surname = 'Cole', 
                //                            register.studentno = '5599999' }
                //               TARGET {   cassandra }",
                //    Active = true
                //});

                ////add field(s) with partial clustered index provided
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "add field(s) with full clustered index provided",
                //    Target = "Redis",
                //    ExecutionTimes = 1,
                //    Script = @"ADD { student }
                //               PROPERTIES { idnumber = '87676556443', 
                //                            title = 'Miss', 
                //                            name = 'Lauren', 
                //                            surname = 'Cole', 
                //                            register.studentno = '5599999' }
                //               TARGET {   cassandra }",
                //    Active = true
                //});

                #endregion

                #region "MongoDb only"

                ////retrieves data from source without filter
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////retrieves data from source with single filter
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with single filter",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON {s.idnumber = '04001104036'}
                //              TARGET {  mongodb }",
                //    Active = true,
                //});

                ////retrieves data from source with more than one filter using AND clause)
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with more than one filter using AND clause",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '04001104036' AND s.gender = 'M'}
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////retrieves data x amount of records
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data x amount of records",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              RESTRICT_TO { 10 }
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////sort data retrieval
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "sort data retrieval",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              ORDER_BY { s.name} 
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////retrieves data with filter and sort by column
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data with filter and sort by column",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '04001104036'}
                //              ORDER_BY { s.name} 
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////retrieves data with multiple filters using AND filter on and sort by column
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data with multiple filters using AND filter on and sort by column",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '04001104036' AND  s.register.studentno = '451782010'}
                //              ORDER_BY { s.name} 
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////retrieves data with multiple filters using OR filter on and sort by column
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data with multiple OR in filters and sort by column",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '35808404617' OR s.idnumber = '21708702176'}
                //              ORDER_BY { s.name} 
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////retrieves data with multiple OR / AND in filters and sort by column
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data with multiple OR/AND in filters and sort by column",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.city, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '35808404617' OR s.idnumber = '21708702176' AND s.gender = 'F' }
                //              ORDER_BY { s.name} 
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////aggregate data using SUM function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using SUM function",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NSUM(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////aggregate data using AVG function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using AVG function",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NAVG(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////aggregate data using COUNT function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using COUNT function",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NCOUNT(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////aggregate data using MIN function 
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using MIN function ",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NMIN(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////retrieves data from source with MAX
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using MAX function ",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, s.transcript.subject, NMAX(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '58602700606'}
                //              TARGET { mongodb }",
                //    Active = true
                //});

                ////modify field(s) with single filter
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modify field(s) with single filter",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Test qqq2', surname = 'Test qq3', initial = 'TT'}
                //               FILTER_ON { idnumber = '04001104036'}
                //               TARGET { mongodb }",
                //    Active = true
                //});

                ////modify field(s) with muliple filters
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modify field(s) with muliple filters",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Micheal', surname = 'Corleone', initial = 'M'}
                //               FILTER_ON { idnumber = '04001104036' AND register.studentno = '451782010'}
                //               TARGET { mongodb }",
                //    Active = true
                //});


                ////add field(s) with full clustered index provided
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "add field(s) with full clustered index provided",
                //    Target = "MongoDB",
                //    ExecutionTimes = 1,
                //    Script = @"ADD { student }
                //               PROPERTIES { 
                //                            idnumber = '87676556443', 
                //                            title = 'Miss', 
                //                            name = 'Lauren', 
                //                            surname = 'Cole', 
                //                            register.studentno = '5599999' }
                //               TARGET {   mongodb }",
                //    Active = true
                //});

                #endregion

                #region "Neo4j only"

                ////retrieves data from source without filter referencing all 7 the nodes
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing all the nodes",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date,s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source without filter referencing 6 out of 7 the nodes
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing 6 out of 7 the nodes",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date }
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source without filter referencing 5 out of 7 the nodes
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing 5 out of 7 the nodes",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date }
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source without filter referencing 4 out of 7 the nodes with optional match
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing 5 out of 7 the nodes with optional match",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date }
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source without filter referencing 4 out of 7 the nodes with non-optional match
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing 5 out of 7 the nodes with non-optional match",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, 
                //                      s.register.course.code, s.register.course.name, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date }
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source without filter referencing 3 out of 7 the nodes
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing 3 out of 7 the nodes",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, 
                //                      s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date }
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source without filter referencing 2 out of 7 the nodes, part 1
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing 2 out of 7 the nodes, part 1",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile}
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source without filter referencing 2 out of 7 the nodes, part 2
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing 2 out of 7 the nodes, part 2",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,s.address.city,
                //                      s.contact.email, s.contact.mobile}
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source without filter referencing 1 out of 7 the nodes
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter referencing 2 out of 7 the nodes, part 2",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,
                //                      s.contact.email, s.contact.mobile}
                //              DATA_MODEL { student AS s}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source with single filter 
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with single filter",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,
                //                      s.contact.email, s.contact.mobile}
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '04001104036'}
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source with AND filter 
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with AND filter",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,
                //                      s.contact.email, s.contact.mobile}
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '04001104036' AND s.gender = 'M'}
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source with OR filter
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source with OR filter",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,
                //                      s.contact.email, s.contact.mobile}
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '21708702176' OR s.gender = 'M'}
                //              RESTRICT_TO { 100 }
                //              TARGET { neo4j }",
                //    Active = true
                //});


                ////retrieves data from source in sort order
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source in sort order",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street,s.address.postalcode,s.address.postaladdress,
                //                      s.contact.email, s.contact.mobile}
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '21708702176' OR s.gender = 'M'}
                //              RESTRICT_TO { 100 }
                //              ORDER_BY { s.surname, s.name} 
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////aggregate data using SUM function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using SUM function",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NSUM(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '21708702176'}
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////aggregate data using AVG function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using AVG function",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NAVG(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '21708702176'}
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////aggregate data using COUNT function
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using COUNT function",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NCOUNT(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '21708702176'}
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////aggregate data using MIN function 
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using MIN function ",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NMIN(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '21708702176'}
                //              TARGET { neo4j }",
                //    Active = true
                //});

                ////retrieves data from source with MAX
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "aggregate data using MAX function ",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH {s.idnumber, s.initial, s.name, s.surname, NMAX(s.transcript.result) }
                //              DATA_MODEL { student AS s }
                //              FILTER_ON { s.idnumber = '21708702176'}
                //              TARGET { neo4j }",
                //    Active = true
                //});

                //modify field(s) with single filter
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modify field(s) with single filter",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Test qqq2', surname = 'Test qq3', initial = 'TT'}
                //               FILTER_ON { idnumber = '21708702176'}
                //               TARGET { neo4j }",
                //    Active = true
                //});

                ////modify field(s) with muliple filters
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "modify field(s) with muliple filters",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"MODIFY { student }
                //               PROPERTIES { name = 'Micheal', surname = 'Corleone', initial = 'M'}
                //               FILTER_ON { idnumber = '04001104036' }
                //               TARGET { neo4j }",
                //    Active = true
                //});


                ////add field(s) with full clustered index provided
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "add field(s) with full clustered index provided",
                //    Target = "Neo4j",
                //    ExecutionTimes = 1,
                //    Script = @"ADD { student }
                //               PROPERTIES { 
                //                            idnumber = '87676556443', 
                //                            title = 'Miss', 
                //                            name = 'Lauren', 
                //                            surname = 'Cole'
                //                            }
                //               TARGET {   neo4j }",
                //    Active = true
                //});
                #endregion

                #region "Fetch from more than one source"

                ////retrieves data from all sources without filter
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from source without filter",
                //    Target = "Cassandra",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date, s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              TARGET { redis, cassandra, mongodb, neo4j }",
                //    Active = true
                //});

                ////retrieves data from all sources with single filter
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from all sources with single filter",
                //    Target = "ALL",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date, s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '21708702176'}
                //              TARGET { redis, cassandra, mongodb, neo4j }",
                //    Active = true
                //});

                ////retrieves data from all sources with AND filters
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from all sources with AND filters",
                //    Target = "ALL",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date, s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '21708702176' AND s.idnumber = '55600308764'}
                //              TARGET { redis, cassandra, mongodb, neo4j }",
                //    Active = true
                //});

                //retrieves data from all sources with OR filters
                //testno++;
                //_registeries.Add(testno, new RegisteryModel
                //{
                //    No = testno,
                //    Description = "retrieves data from all sources with OR filters",
                //    Target = "ALL",
                //    ExecutionTimes = 1,
                //    Script = @"FETCH { s.title,s.idnumber,s.preferredname,s.initial,s.name, s.surname, s.dateofbirth, s.gender, s.address.streetno,
                //                      s.address.street, s.address.postalcode,s.address.postaladdress,s.address.city,s.address.country.code,s.address.country.name,
                //                      s.contact.email, s.contact.mobile,s.register.studentno, s.register.faculty.code, s.register.faculty.name, 
                //                      s.register.course.code, s.register.course.name, s.register.subject.code, s.register.subject.name, 
                //                      s.register.subject.cost, s.register.subject.duration, s.register.username,s.register.password, s.register.type, s.register.ipaddress,
                //                      s.register.date, s.transcript.subject, s.transcript.result, s.transcript.symbol }
                //              DATA_MODEL { student AS s}
                //              FILTER_ON { s.idnumber = '21708702176' OR s.idnumber = '55600308764'}
                //              TARGET { redis, cassandra, mongodb, neo4j }",
                //    Active = true
                //});


                #endregion

                #endregion

                #region "Modify data for supported targets"

                #endregion

                #region "Add data for supported targets"

                #endregion
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Create test scenario errors...");
            }
        }
    }
}
