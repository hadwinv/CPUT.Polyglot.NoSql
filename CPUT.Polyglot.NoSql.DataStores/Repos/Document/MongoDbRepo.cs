using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Native.MongoDB;
using CPUT.Polyglot.NoSql.Models.Translator;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Document
{
    public class MongoDbRepo : IMongoDbRepo
    {
        private readonly IMongoDBBridge _connector;

        public MongoDbRepo(IMongoDBBridge connector)
        {
            _connector = connector;
        }
        public Models.Result Execute(Constructs construct)
        {
            try
            {
                if (construct.Query != null)
                {
                    foreach (var query in construct.Query)
                    {
                        _connector.Connect().RunCommand(query);
                    }
                }

                int i = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
            return null;
        }

        #region Data Load

        public void Load(List<UDataset> dataset)
        {
            List<CourseModel> documents = new List<CourseModel>();
            List<StudentsModel> peoples = new List<StudentsModel>();

            SubjectModel subjectModel = null;
            CourseModel courseModel = null;
            FacultiesModel facultyModel = null;
            StudentsModel peopleModel = null;

            int count = 1;

            try
            {
                var connection = _connector.Connect();

                connection.DropCollection("courses");
                connection.DropCollection("students");

                var courseCollection = connection.GetCollection<CourseModel>("courses");

                foreach (var faculty in dataset[0].Faculties)
                {
                    facultyModel = new FacultiesModel
                    {
                        id = faculty.Id,
                        name = faculty.Description,
                        head = "test name",
                        contact = new ContactModel
                        {
                            id = faculty.Id,
                            phone = "0722794007",
                            email = "test@test.com",
                            address = new AddressModel
                            {
                                id = faculty.Id,
                                city = "ttrtr",
                                code = "3332",
                                country = "Suth",
                                street = "14 test strate"
                            }
                        }
                    };

                    foreach (var course in faculty.Courses)
                    {
                        foreach (var subject in course.Subjects)
                        {
                            subjectModel = new SubjectModel
                            {
                                id = subject.Id,
                                name = subject.Description,
                                cost = subject.Price,
                                duration = 6,
                            };
                        }

                        courseModel = new CourseModel
                        {
                            id = course.Id,
                            name = course.Description,
                            faculty = facultyModel,
                            subject = subjectModel
                        };

                        documents.Add(courseModel);
                    }
                }

                courseCollection.InsertMany(documents.ToArray());

                var studentCollection = connection.GetCollection<StudentsModel>("students");

                foreach (var student in dataset[0].Students)
                {
                    peopleModel = new StudentsModel
                    {
                        id = count,
                        name = student.Name,
                        surname = student.Surname,
                        id_number = student.IdNumber,
                        date_of_birth = DateTime.Parse(student.DOB),
                        contact = new ContactModel
                        {
                            id = count,
                            address = new AddressModel
                            {
                                id = student.Address.Id,
                                street = student.Address.Street,
                                code = string.Empty,
                                suburb = string.Empty,
                                city = student.Address.City,
                                country = student.Address.Location.Country
                            },
                            email = student.Email,
                            phone = student.MobileNo
                        },
                        register = new EnrollmentModel
                        {
                            id = count,
                            registration_date = student.Profile.RegistrationDate.Value,
                            status = "Registered",
                            course = new CourseModel
                            {
                                faculty = new FacultiesModel
                                {
                                    id = count,
                                    name = string.Empty,
                                    contact = new ContactModel
                                    {
                                        address = new AddressModel
                                        {
                                            id = count,
                                            city = string.Empty,
                                            country = string.Empty,
                                            code = string.Empty,
                                            street = string.Empty,
                                            suburb = string.Empty
                                        },
                                        email = string.Empty,
                                        phone = string.Empty
                                    },
                                    head = string.Empty,
                                    shortcode = string.Empty
                                },
                                name = student.Profile.Course.Description
                            }
                        },
                    };

                    count++;
                    peoples.Add(peopleModel);
                }

                studentCollection.InsertMany(peoples.ToArray());
            }
            catch (MongoException ex)
            {
                Console.WriteLine($"MongoException - {ex.Message}");
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
