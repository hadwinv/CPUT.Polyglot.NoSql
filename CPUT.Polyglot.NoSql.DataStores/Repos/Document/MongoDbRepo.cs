using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Native.MongoDB;
using CPUT.Polyglot.NoSql.Models.Translator;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

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

                connection.DropCollection("students");

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
                        register = new RegisterModel
                        {
                            id = count,
                            registration_date = student.Profile.RegistrationDate.Value,
                            status = "Registered",
                        },
                    };

                    
                    peopleModel.register.course = new CourseModel();
                    peopleModel.register.course.subjects = new List<SubjectModel>();
                    peopleModel.register.course.id = student.Profile.Course.Id;
                    peopleModel.register.course.name = student.Profile.Course.Description;

                    var found = false;

                    foreach(var faculties in dataset[0].Faculties)
                    {
                        foreach(var courses in faculties.Courses.Where(x => x.Code == student.Profile.Course.Code))
                        {
                            found = true;

                            peopleModel.register.course.faculty = new FacultiesModel
                            {
                                id = faculties.Id,
                                shortcode = faculties.Code,
                                name = faculties.Description,
                                head = "John Doe",
                                contact = new ContactModel
                                {
                                    id = faculties.Id,
                                    phone = "0731234567",
                                    email = "john@doe.com",
                                    address = new AddressModel
                                    {
                                        id = faculties.Id,
                                        city = "Cape Town",
                                        code = "8000",
                                        country = "South Africa",
                                        street = "14 John Doe street"
                                    }
                                }
                            };
                        }

                        if(found)
                            break;
                    }
                   
                    foreach (var subject in student.Profile.Course.Subjects)
                    {
                        peopleModel.register.course.subjects.Add(new SubjectModel
                        {
                            id = subject.Id,
                            name = subject.Description,
                            shortcode = subject.Code,
                            cost = subject.Price,
                            duration = subject.Duration
                        });
                    }
                    
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
