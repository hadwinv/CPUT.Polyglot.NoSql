using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Translator;
using Neo4j.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Graph
{
    public class Neo4jRepo : INeo4jRepo
    {
        private readonly INeo4jBridge _connector;

        public Neo4jRepo(INeo4jBridge connector) 
        {
            _connector = connector;
        }

        public Models.Result Execute(Constructs construct)
        {
            Models.Result result = null;

            try
            {
                var connection = _connector.Connect();

                var session = connection.AsyncSession(configBuilder => configBuilder.WithDatabase("enrollmentdb"));

                if (construct.Query != null)
                {
                    session.WriteTransactionAsync(async tx =>
                    {
                        var response = tx.RunAsync(construct.Query);

                        result = new Models.Result
                        {
                            Data = response,
                            Message = "OK",
                            Success = true
                        };
                    }).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");

                result = new Models.Result
                {
                    Data = null,
                    Message = ex.Message,
                    Success = false
                };
            }

            return result;
        }

        #region Data Load

        public void Load(List<UDataset> dataset)
        {
            var query = string.Empty;

            List<string> faculties = new List<string>();
            List<string> courses = new List<string>();
            List<string> country = new List<string>();
            List<string> city = new List<string>();

            IDriver connection = null;
            IAsyncSession session = null;
            int count = 0;

            try
            {
                connection = _connector.Connect();

                session = connection.AsyncSession(configBuilder => configBuilder.WithDatabase("enrollmentdb"));

                var deleteAll = @"MATCH (n) DETACH DELETE n;";

                // clear all nodes
                session.WriteTransactionAsync(async tx =>
                {
                    var result = tx.RunAsync(deleteAll).Result;
                });

                //faculty
                foreach (var faculty in dataset[0].Faculties)
                {
                    if (!faculties.Contains(faculty.Code))
                    {
                        query += "CREATE (" + faculty.Code + ":Faculty {name: '" + faculty.Description + "', code: '" + faculty.Code + "'})";
                    }

                    foreach (var course in faculty.Courses)
                    {
                        if (!courses.Contains(course.Code))
                        {
                            query += "CREATE (" + course.Code + ":Course {name: '" + course.Description + "', code: '" + course.Code + "'})";

                            query += "CREATE (" + course.Code + ")-[:OFFERED_IN]->(" + faculty.Code + ") ";
                        }

                        foreach (var subject in course.Subjects)
                        {
                            query += "CREATE (" + subject.Code + "_" + course.Code + ":Subject {name: '" + subject.Description + "', code: '" + subject.Code + "', cost: " + subject.Price + "}) ";

                            query += "CREATE (" + course.Code + ")-[:CONTAINS]->(" + subject.Code + "_" + course.Code + ")";
                        }

                        courses.Add(course.Code);
                    }
                    faculties.Add(faculty.Code);
                }

                session.WriteTransactionAsync(async tx =>
                {
                    var result = tx.RunAsync(query);
                }).Wait();

                foreach (var student in dataset[0].Students)
                {
                    query = string.Empty;

                    query += "CREATE (" + student.Name + "_" + student.Surname.Trim() + "_" + student.Id + ":Pupil { title: '" + student.Title + "', name: '" + student.Name + "', surname: '" + student.Surname + "', idnumber: '" + student.IdNumber + "', dob: '" + student.DOB + "', gender: '" + student.Gender + "', email: '" + student.Email + "', mobile: '" + student.MobileNo + "', language: '" + student.Language + "'}) ";

                    query += "CREATE (" + student.Surname.Trim() + "_" + student.Id + ":Progress {studentno: '" + student.Profile.StudentNo + "', name: '" + student.Name + "', marks: '" + JsonConvert.SerializeObject(student.Marks) + "'}) ";

                    if (!country.Contains(student.Address.Location.CountryCode))
                        query += "CREATE (" + student.Address.Location.CountryCode + ":Country {id: " + student.Address.Location.Id + ", code: '" + student.Address.Location.CountryCode + "'" + ", name: '" + student.Address.Location.Country + "'}) ";

                    if (!city.Contains(student.Address.City))
                        query += "CREATE (" + student.Address.City.Replace(" ", "").Replace(".", "") + ":City {name: '" + student.Address.City + "'}) ";

                    country.Add(student.Address.Location.CountryCode);
                    city.Add(student.Address.City);

                    session.WriteTransactionAsync(async tx =>
                    {
                        var result = tx.RunAsync(query);
                    }).Wait();

                    count++;
                }

                query = string.Empty;

                //enrolled in
                foreach (var student in dataset[0].Students)
                {
                    if (student.Profile.Course != null)
                        query = "MATCH (n:Pupil), (x:Course) WHERE n.idnumber = '" + student.IdNumber + "' AND x.code = '" + student.Profile.Course.Code + "' MERGE (n)-[r:ENROLLED_IN]->(x) ";

                    session.WriteTransactionAsync(async tx =>
                    {
                        var result = tx.RunAsync(query);
                    }).Wait();
                }
                //citizen of
                foreach (var student in dataset[0].Students)
                {
                    query = "MATCH (n:Pupil), (x:Country) WHERE n.idnumber = '" + student.IdNumber + "' AND x.code = '" + student.Address.Location.CountryCode + "' MERGE (n)-[co:CITIZEN_OF]->(x)";

                    session.WriteTransactionAsync(async tx =>
                    {
                        var result = tx.RunAsync(query);
                    }).Wait();
                }
                //lives in
                foreach (var student in dataset[0].Students)
                {
                    query = "MATCH (n:Pupil), (x:City) WHERE n.idnumber = '" + student.IdNumber + "' AND x.name = '" + student.Address.City + "' MERGE (n)-[li:LIVES_IN]->(x) ";

                    session.WriteTransactionAsync(async tx =>
                    {
                        var result = tx.RunAsync(query);
                    }).Wait();
                }
                //located in
                var location = new List<string>();

                foreach (var student in dataset[0].Students)
                {
                    if (!location.Contains(student.Address.Location.CountryCode + ":" + student.Address.City))
                    {
                        query = "MATCH  (x:City), (n:Country) WHERE n.code = '" + student.Address.Location.CountryCode + "' AND x.name = '" + student.Address.City + "' MERGE (x)-[li:IS_LOCATED_IN]->(n) ";

                        session.WriteTransactionAsync(async tx =>
                        {
                            var result = tx.RunAsync(query);
                        }).Wait();
                    }

                    location.Add(student.Address.Location.CountryCode + ":" + student.Address.City);
                }

                //transcript
                foreach (var student in dataset[0].Students)
                {
                    query = "MATCH  (x:Pupil), (n:Progress) WHERE x.idnumber = '" + student.IdNumber + "' AND n.studentno = '" + student.Profile.StudentNo + "' MERGE (x)-[t:TRANSCRIPT]->(n) ";

                    session.WriteTransactionAsync(async tx =>
                    {
                        var result = tx.RunAsync(query);
                    }).Wait();
                }
            }
            catch (Neo4jException ex)
            {
                Console.WriteLine($"Neo4jException - {ex.Message}");
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
