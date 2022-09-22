using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Schema._data.prep;
using Neo4j.Driver;
using System;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Graph
{
    public class Neo4jRepo : INeo4jRepo
    {
        private readonly IDriver _driver;

        public Neo4jRepo(IDriver driver) 
        {
            _driver = driver;
        }

        public void CreateGraphDB(List<UDataset> dataset)
        {
            var query = string.Empty;

            List<string> faculties = new List<string>();
            List<string> courses = new List<string>();
            List<string> country = new List<string>();
            List<string> city = new List<string>();

            try
            {
                var session = _driver.AsyncSession(configBuilder => configBuilder.WithDatabase("enrollmentdb"));

                var deleteAll = @"MATCH (n) DETACH DELETE n";

                // remove all nodes
                session.WriteTransactionAsync(async tx =>
                {
                    var result = tx.RunAsync(deleteAll).Result;
                });

                


                
                //var newBook = new Book { Id = uniqueId, Title = title, CategoryCodes = category };
                //client.Cypher
                //    .Merge("(book:Book { Id: {uniqueId} ,Title:{title},CategoryCodes:{category}})")
                //    .OnCreate()
                //    .Set("book = {newBook}")
                //    .WithParams(new
                //    {
                //        uniqueId = newBook.Id,
                //        title = newBook.Title,
                //        category = newBook.CategoryCodes,
                //        newBook
                //    })
                //    .ExecuteWithoutResults();

                //uniqueId++;

                foreach (var faculty in dataset[0].Faculties)
                {
                    if (!faculties.Contains(faculty.Code))
                    {
                        query += "CREATE (" + faculty.Code + ":Faculty {name: '" + faculty.Description + "', code: '" + faculty.Code + "'}) ";
                    }

                    //foreach (var course in faculty.Courses)
                    //{
                    //    if (!courses.Contains(course.Code))
                    //    {
                    //        query += "CREATE (" + course.Code + ":Course {name: '" + course.Description + "', code: '" + course.Code + "'}) ";

                    //        query += "CREATE (" + course.Code + ")-[:OFFERED_IN]->(" + faculty.Code + ") ";
                    //    }

                    //    foreach (var subject in course.Subjects)
                    //    {
                    //        query += "CREATE (" + subject.Code + "_" + course.Code + ":Subject {name: '" + subject.Description + "', code: '" + subject.Code + "', cost: " + subject.Price + "}) ";

                    //        query += "CREATE (" + course.Code + ")-[:CONTAINS]->(" + subject.Code + "_" + course.Code + ") ";
                    //    }

                    //    courses.Add(course.Code);
                    //}
                    //faculties.Add(faculty.Code);
                }

                int count = 0;

                //foreach (var student in dataset[0].Students)
                //{
                //    if (count > 20)
                //        break;

                //    var uniqueStudent = student.Name + "_" + student.Id;

                //    query += "CREATE (" + uniqueStudent + ":Pupil { title: '" + student.Title + "', name: '" + student.Name + "', surname: '" + student.Surname + "', idnumber: '" + student.IdNumber + "', dob: '" + student.DOB + "', gender: '" + student.Gender + "', email: '" + student.Email + "', mobile: '" + student.MobileNo + "', language: '" + student.Language + "'}) ";

                //    if (student.Profile != null)
                //    {
                //        //query += "CREATE (" + uniqueStudent + ")-[:ENROLLED_IN {profilecreated: '" + student.Profile.CreatedDate + "', profileid: '" + student.Profile.ProfileId + "'}]->(" + student.Profile.Course.Code + ") ";
                //    }

                //    foreach (var marks in student.Marks)
                //    {
                //        var uniqueMarks = student.Name + student.Surname.Substring(0, 1) + marks.Id.ToString();

                //        query += "CREATE (" + uniqueMarks + ":Progress {id: " + marks.Id + ", subject: '" + marks.Subject + "', grade: '" + marks.Grade + "', score: " + marks.Score + "}) ";

                //        query += "CREATE (" + uniqueStudent + ")-[:TRANSCRIPT]->(" + uniqueMarks + ") ";

                //        query += "CREATE (" + uniqueMarks + ")-[:REGISTERED]->(" + marks.SubjectCode + "_" + marks.CourseCode + ") ";
                //    }

                //    if (!country.Contains(student.Address.Location.CountryCode))
                //        query += "CREATE (" + student.Address.Location.CountryCode + ":Country {id: " + student.Address.Location.Id + ", name: '" + student.Address.Location.Country + "'}) ";

                //    query += "CREATE (" + uniqueStudent + ")-[:CITIZEN_OF]->(" + student.Address.Location.CountryCode + ") ";

                //    if (!country.Contains(student.Address.City))
                //        query += "CREATE (" + student.Address.City.Replace(" ", "").Replace(".", "") + ":City {name: '" + student.Address.City + "'}) ";

                //    query += "CREATE (" + student.Address.City.Replace(" ", "").Replace(".", "") + ")-[:IS_LOCATED_IN { province: '" + student.Address.Location.Province + "'}]->(" + student.Address.Location.CountryCode + ") ";

                //    query += "CREATE (" + uniqueStudent + ")-[:LIVES_IN { address: '" + student.Address.PostalAddress + "'}]->(" + student.Address.City.Replace(" ", "").Replace(".", "") + ") ";

                //    country.Add(student.Address.Location.CountryCode);
                //    country.Add(student.Address.City);

                //    count++;
                //}

                session.WriteTransactionAsync(async tx =>
                {
                    var result = tx.RunAsync(query);
                }).Wait();
            }
            // Capture any errors along with the query and data for traceability
            catch (Neo4jException ex)
            {
                //Console.WriteLine($"{query} - {ex}");
                throw;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"{query} - {ex}");
                throw;
            }

            //throw new NotImplementedException();
        }

        
    }
}
