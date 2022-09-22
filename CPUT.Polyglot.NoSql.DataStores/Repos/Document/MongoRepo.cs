using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Schema._data.prep;
using CPUT.Polyglot.NoSql.Schema.Local.MongoDB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Document
{
    public class MongoRepo : IMongoRepo
    {
        private readonly IMongoDatabase _database;

        public MongoRepo(IMongoDatabase database)
        {
            _database = database;
        }
        public void CreateDocumentDB(List<UDataset> dataset)
        {
            SubjectModel subjectd = null;
            CourseModel coursed = null;
            FacultiesModel facultyd = null;
            var documents = new List<CourseModel>();

            try
            {
                _database.DropCollection("courses");
                _database.DropCollection("students");
                
                var courseCollection = _database.GetCollection<CourseModel>("courses");

                foreach (var faculty in dataset[0].Faculties)
                {
                    facultyd = new FacultiesModel {
                        id = faculty.Id,
                        name = faculty.Description,
                        head = "test name",
                        contact = new ContactModel {
                            id = faculty.Id,
                            phone = "0722794007",
                            email = "test@test.com",
                            address = new AddressModel
                            {
                                id = faculty.Id,
                                city = "ttrtr",
                                code = "3332",
                                country ="Suth",
                                street = "14 test strate"
                            }
                        }
                    };

                    foreach (var course in faculty.Courses)
                    {
                        foreach (var subject in course.Subjects)
                        {
                            subjectd = new SubjectModel
                            {
                                id = subject.Id,
                                name = subject.Description,
                                cost = subject.Price,
                                duration = 6,
                            };
                        }

                        coursed = new CourseModel
                        {
                            id=course.Id,
                            name = course.Description,
                            faculty = facultyd,
                            subject = subjectd
                        };


                        documents.Add(coursed);

                    }
                }

                courseCollection.InsertMany(documents.ToArray());


                var studentCollection = _database.GetCollection<PeopleModel>("students");

                foreach (var faculty in dataset[0].Students)
                {

                    //StudentDocumentModel
                }

                courseCollection.InsertMany(documents.ToArray());


            }
            // Capture any errors along with the query and data for traceability
            catch (MongoException ex)
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
