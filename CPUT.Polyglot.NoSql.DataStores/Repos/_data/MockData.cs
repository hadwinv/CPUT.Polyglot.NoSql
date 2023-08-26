using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using MongoDB.Driver;
using Neo4j.Driver;
using StackExchange.Redis;
using CPUT.Polyglot.NoSql.Interface.Delegator.Adaptors;
using System.Reflection;
using CPUT.Polyglot.NoSql.Models._data.prep.MongoDb;
using CPUT.Polyglot.NoSql.Models.Native._data.prep.Redis;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models._data;
using Path = System.IO.Path;
using MongoDB.Driver.Linq;
using CPUT.Polyglot.NoSql.Models.Views.Bindings;
using CPUT.Polyglot.NoSql.Models.Views.Bindings.Loads;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Transform;
using Cassandra;
using System.Xml.Linq;
using System.Net;
using CPUT.Polyglot.NoSql.Models._data.prep.Neo4j;

namespace CPUT.Polyglot.NoSql.DataStores.Repos._data
{
    public class MockData : IMockData
    {
        private ICassandraBridge _cassandraConnector;
        private IMongoDBBridge _mongoDbConnector;
        private INeo4jBridge _neo4jConnector;
        private IRedisBridge _redisConnector;

        public MockData(
            ICassandraBridge cassandraConnector, IMongoDBBridge mongoDbConnector, 
            INeo4jBridge neo4jConnector, IRedisBridge redisConnector)
        {
            _cassandraConnector = cassandraConnector;
            _mongoDbConnector = mongoDbConnector;
            _neo4jConnector = neo4jConnector;
            _redisConnector = redisConnector;
        }

        public void GenerateData()
        {
            try
            {
                //folder, where a file is created.
                var @base = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\testdata";

                if(!Directory.Exists(@base))
                    Directory.CreateDirectory(@base);

                string faculties = "_faculties.txt";
                var fcsv = new StringBuilder();

                string studentprofile = "_students.txt";
                var pcsv = new StringBuilder();

                //load mock data
                var dataset = GetMockDataset();

                #region "faculty file"

                if (!File.Exists(Path.Combine(@base, faculties)))
                {
                    using (StreamWriter fstream = new StreamWriter(Path.Combine(@base, faculties)))
                    {
                        var header = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                            "facultycode",
                                            "facultydescription",
                                            "coursecode",
                                            "coursedescription",
                                            "subjectkey",
                                            "subjectdescription",
                                            "subjectcost",
                                            "subjectterm");
                        //write header
                        fcsv.AppendLine(header);

                        foreach (var course in dataset.Courses)
                        {
                            foreach (var subject in course.subject)
                            {
                                var line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                    course.facultycode,
                                    course.faculty,
                                    course.code,
                                    course.name,
                                    subject.short_code,
                                    subject.name,
                                    subject.cost,
                                    subject.term);
                                fcsv.AppendLine(line);
                            }
                        }

                        fstream.WriteLine(fcsv.ToString());
                    }
                }

                #endregion

                #region "student profile file"

                if (!File.Exists(Path.Combine(@base, studentprofile)))
                {
                    using (StreamWriter sstream = new StreamWriter(Path.Combine(@base, studentprofile)))
                    {
                        var header = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}|{31}|{32}|{33}|{34}|{35}|{36}|{37}",
                                            "idnumber",
                                            "title",
                                            "name",
                                            "surname",
                                            "dob",
                                            "gender",
                                            "email",
                                            "mobileno",
                                            "language",
                                            "studentno",
                                            "username",
                                            "password",
                                            "ipaddress",
                                            "registrationdate",
                                            "graduateddate",
                                            "profilefacultycode",
                                            "profilefacultydescription",
                                            "profilecoursecode",
                                            "profilecoursedescription",
                                            "profilesubjectkey",
                                            "profilesubjectdescription",
                                            "profilesubjectcost",
                                            "profilesubjectterm",
                                            "streetid",
                                            "streetno",
                                            "street",
                                            "streetaddress",
                                            "postaladdress",
                                            "postalcode",
                                            "city",
                                            "locationid",
                                            "province",
                                            "countrycode",
                                            "country",
                                            "marksubjectkey",
                                            "markesubjectdescription",
                                            "markscore",
                                            "markgrade");
                        //write header
                        pcsv.AppendLine(header);

                        foreach (var student in dataset.Students)
                        {
                            foreach (var subject in student.Profile.Course.subject)
                            {
                                var line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}|{20}|{21}|{22}|{23}|{24}|{25}|{26}|{27}|{28}|{29}|{30}|{31}|{32}|{33}|{34}|{35}|{36}|{37}",
                                        student.IDNumber,
                                        student.Title,
                                        student.FirstName,
                                        student.LastName,
                                        DateTime.Parse(student.DOB).ToString("yyyy/MM/dd"),
                                        student.Gender,
                                        student.Email,
                                        student.PhoneNo,
                                        student.Language,
                                        student.Profile.StudentNo,
                                        student.Profile.Username,
                                        student.Profile.Password,
                                        student.Profile.IPAddress,
                                        student.Profile.RegistrationDate?.ToString("yyyy/MM/dd"),
                                        student.Profile.GraduatedDate?.ToString("yyyy/MM/dd"),
                                        student.Profile.Course.facultycode,
                                        student.Profile.Course.faculty,
                                        student.Profile.Course.code,
                                        student.Profile.Course.name,
                                        subject.short_code,
                                        subject.name,
                                        subject.cost,
                                        subject.term,
                                        student.Address.Id,
                                        student.Address.StreetNo,
                                        student.Address.Street,
                                        student.Address.StreetAddress,
                                        student.Address.PostalAddress,
                                        student.Address.PostalCode,
                                        student.Address.City,
                                        student.Address.Location.Id,
                                        student.Address.Location.Province,
                                        student.Address.Location.CountryCode,
                                        student.Address.Location.Country,
                                        subject.short_code,
                                        subject.name,
                                        subject.mark,
                                        subject.grade);

                                pcsv.AppendLine(line);
                            }
                        }

                        sstream.WriteLine(pcsv.ToString());
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
        }

        public void Load()
        {
            try
            {
                var dataset = LoadDataset();

                //data test data
                LoadRedis(dataset);

                LoadCassandra(dataset);

                LoadMongoDB(dataset);

                LoadNeo4j(dataset);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
        }

        #region Redis

        private void LoadRedis(DatabaseModel dataset)
        {
            try
            {
                var redis = _redisConnector.Connect();

                //clear all keys
                _redisConnector.Flush();
                
                var pidnumber = string.Empty;
                var iterator = 1;

                foreach (var student in dataset.Students)
                {
                    if(pidnumber != student.idnumber)
                    {
                        var user = new rUser
                        {
                            user_id = iterator.ToString(),
                            identity_number = student.idnumber,
                            student_number = student.register.studentno,
                            title = student.title,
                            other_name = student.preferredname,
                            first_name = student.name,
                            last_name = student.surname,
                            birth_date = DateTime.Parse(student.dateofbirth),
                            gender = student.gender,
                            user_name = student.register.username,
                            psw = student.register.password,
                            ip_address = student.register.ipaddress,
                            device = student.name,
                            session_id = Guid.NewGuid().ToString(),
                            login_date = DateTime.Now.AddMinutes(-30),
                            logout_date = DateTime.Now,
                            audit_date = DateTime.Now,
                            city = student.address.city,
                            country = student.address.country.name
                        };

                        var jsonConvert = JsonConvert.SerializeObject(user);

                        redis.StringSet(key: student.idnumber, value: jsonConvert, expiry: new TimeSpan(5, 0, 0, 0));

                        iterator++;
                    }
                    

                    pidnumber = student.idnumber;
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
                if (_redisConnector != null)
                    _redisConnector.Disconnect();
            }
        }

        #endregion

        #region Cassandra

        private void LoadCassandra(DatabaseModel dataset)
        {
            string query = string.Empty;
          
            try
            {
                //call to create structure
                CreateDocumentSchema();

                var pidnumber = string.Empty;
                var pcourse = string.Empty;

                string address = string.Empty;
                string registered = string.Empty;
                string grades = string.Empty;
                string subjects = string.Empty;

                int iterator = 0;
                int next = 0;

                //data insert
                foreach (var student in dataset.Students)
                {
                    if (iterator < (dataset.Students.Count - 1))
                        next = iterator + 1;


                    if ((iterator < next && student.idnumber != dataset.Students[next].idnumber) || next == iterator)
                    {
                        subjects = string.Empty;
                        grades = string.Empty;
                    }

                    subjects = "{ " +
                                      "descr: '" + student.register.subject.name + "', " +
                                      "price: " + student.register.subject.cost + "," +
                                      "period: " + student.register.subject.duration +
                                  "}";

                    grades = "{ " +
                                      "subject: '" + student.register.subject.name + "', " +
                                      "marks: " + student.transcript.result + "," +
                                      "symbol: '" + student.transcript.symbol + "'" +
                                  "}";

                    address = @"{
                                            streetno: " + student.address.streetno + "," +
                                           "streetname: '" + student.address.street + "'," +
                                           "city: '" + student.address.city + "'," +
                                           "postaladdress: '" + student.address.postaladdress + "'," +
                                           "postalcode: '" + student.address.postalcode + "'," +
                                           "province: '" + student.address.province + "'," +
                                           "country: '" + student.address.country.name + "'" +
                                   "}";

                    registered = (@"{ 
                                faculty : '" + student.register.faculty.name + "'," +
                            "course : '" + student.register.course.name + "'," +
                            "registerdate : ''," +
                            "subject :  _here_ }").Replace("_here_", subjects);
                    //" + student.register.date + "

                    query = string.Format(@"INSERT INTO cput.student (id, idno, studentno, title, aka, initials, firstname, lastname, dob, genderid, email, cellno, address, registered, grades ) 
                                    VALUES( " +
                                "'" + iterator.ToString() + "', " +
                                "'" + student.idnumber + "', " +
                                "'" + student.register.studentno + "'," +
                                "'" + student.title + "'," +
                                "'" + student.preferredname + "'," +
                                "'" + student.name.Substring(0, 1) + "'," +
                                "'" + student.name + "'," +
                                "'" + student.surname + "'," +
                                "'" + student.dateofbirth + "'," +
                                "'" + student.gender + "'," +
                                "'" + student.contact.email + "'," +
                                "'" + student.contact.mobile + "'," +
                                "{0},{1}, {2} );", address, registered, grades);

                    _cassandraConnector.Connect().Execute(query);

                    pidnumber = student.idnumber;
                    pcourse = student.register.course.name;
                    iterator++;
                }

                _cassandraConnector.Connect().Execute("CREATE INDEX idno_idx ON cput.student(idno);");
                _cassandraConnector.Connect().Execute("CREATE INDEX studentno_idx ON cput.student(studentno);");
                _cassandraConnector.Connect().Execute("CREATE INDEX genderid_idx ON cput.student(genderid);");
                _cassandraConnector.Connect().Execute("CREATE INDEX lastname_idx ON cput.student(lastname);");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CassandraException - {ex.Message}");
                throw;
            }
            finally
            {
                if (_cassandraConnector != null)
                    _cassandraConnector.Disconnect();
            }
        }

        private void CreateDocumentSchema()
        {
            _cassandraConnector.Connect().Execute("CREATE KEYSPACE IF NOT EXISTS cput WITH REPLICATION = { 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 };");

            _cassandraConnector.Connect().Execute("DROP TABLE IF EXISTS cput.student;");

            _cassandraConnector.Connect().Execute("DROP TYPE IF EXISTS cput.address;");
            _cassandraConnector.Connect().Execute("DROP TYPE IF EXISTS cput.registered;");
            _cassandraConnector.Connect().Execute("DROP TYPE IF EXISTS cput.grades;");
            _cassandraConnector.Connect().Execute("DROP TYPE IF EXISTS cput.subject;");

            string subject = @"CREATE TYPE IF NOT EXISTS cput.subject( 
                    descr varchar,
                    price decimal,
                    period int);";

            _cassandraConnector.Connect().Execute(subject);

            string address = @"CREATE TYPE IF NOT EXISTS cput.address( 
                    streetno int,
                    streetname varchar,
                    city varchar,
                    postaladdress varchar,
                    postalcode varchar,
                    province varchar,
                    country varchar);";

             _cassandraConnector.Connect().Execute(address);

            string registered = @"CREATE TYPE IF NOT EXISTS cput.registered( 
                    faculty varchar,
                    course varchar,
                    subject frozen<subject>,
                    registerdate timestamp);";

            _cassandraConnector.Connect().Execute(registered);

            string grades = @"CREATE TYPE IF NOT EXISTS cput.grades(
                    subject varchar,
                    marks decimal,
                    symbol varchar
                    ); ";

            _cassandraConnector.Connect().Execute(grades);

            string student = @"CREATE TABLE IF NOT EXISTS cput.student(
                    id varchar, 
                    idno varchar, 
                    studentno varchar, 
                    title varchar, 
                    aka varchar, 
                    initials varchar, 
                    firstname varchar,
                    lastname varchar,
                    dob varchar,
                    genderid varchar, 
                    email varchar,
                    cellno varchar,
                    address frozen<address>,
                    registered frozen<registered>,
                    grades frozen<grades>,
                    PRIMARY KEY (id)
                    ); ";

            _cassandraConnector.Connect().Execute(student);
        }

        #endregion

        #region MongoDB

        private void LoadMongoDB(DatabaseModel dataset)
        {
            var peoples = new List<mStudents>();

            try
            {
                var connection = _mongoDbConnector.Connect();

                connection.DropCollection("students");

                var studentCollection = connection.GetCollection<mStudents>("students");

                var pidnumber = string.Empty;

                int iterator = 0;
                int next = 0;

                var subjects = new List<mSubject>();

                foreach (var student in dataset.Students)
                {
                    if (iterator < (dataset.Students.Count - 1))
                        next = iterator + 1;

                    if ((iterator < next && student.idnumber != dataset.Students[next].idnumber) || next == iterator)
                    {
                        var students = new mStudents
                        {
                            student_id = iterator.ToString(),
                            student_no = student.register.studentno.ToString(),
                            id_number = student.idnumber,
                            title = student.title,
                            init = student.name.Substring(0, 1),
                            name = student.name,
                            surname = student.surname,
                            date_of_birth = DateTime.Parse(student.dateofbirth),
                            gender_identity = student.gender,
                            contact = new mContact
                            {
                                email_address = student.contact.email,
                                phone = student.contact.mobile
                            },
                            address = new mAddress
                            {
                                number = int.Parse(student.address.streetno),
                                street = student.address.street,
                                city = student.address.city,
                                country = student.address.country.name
                            },
                            enroll = new mEnroll
                            {
                                faculty = new mFaculty
                                {
                                    short_code = student.register.faculty.code,
                                    name = student.register.faculty.name
                                },
                                course = new mCourse
                                {
                                    short_code = student.register.course.code,
                                    name = student.register.course.name
                                },
                                subject = subjects,
                                enrollment_type = student.register.type,
                                enrollment_date = DateTime.Parse(student.register.date)
                            }
                        };

                        peoples.Add(students);

                        subjects = new List<mSubject>();
                    }

                    subjects.Add(new mSubject
                    {
                        short_code = student.register.subject.code,
                        name = student.register.subject.name,
                        price = decimal.Parse(student.register.subject.cost),
                        duration = int.Parse(student.register.subject.duration),
                    });

                    iterator++;
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
                Console.WriteLine($"MongoException - {ex.Message}");
                throw;
            }
            finally
            {
                if (_mongoDbConnector != null)
                    _mongoDbConnector.Disconnect();
            }
        }

        #endregion

        #region Neo4j

        public void LoadNeo4j(DatabaseModel dataset)
        {
            var query = string.Empty;

            IDriver connection = null;
            IAsyncSession session = null;

            try
            {
                connection = _neo4jConnector.Connect();

                session = connection.AsyncSession(configBuilder => configBuilder.WithDatabase("enrollmentdb"));

                var deleteAll = @"MATCH (n) DETACH DELETE n;";

                //clear all nodes
                session.WriteTransactionAsync(async tx =>
                {
                    var result = tx.RunAsync(deleteAll);
                }).Wait();

                //faculty
                foreach (var faculty in dataset.Faculties)
                {
                    query = "CREATE (" + faculty.code + ":faculty {description: '" + faculty.name + "', key: '" + faculty.code + "'})";

                    var course = dataset.Courses[faculty.code];

                    if (course != null)
                    {
                        query += "CREATE (" + course.code + ":course {description: '" + course.name + "', key: '" + course.code + "'})";

                        query += "CREATE (" + course.code + ")-[:OFFERED_IN]->(" + faculty.code + ")";


                        foreach (var subject in dataset.Subjects[course.code])
                        {
                            query += "CREATE (" + subject.code + "_" + course.code + ":subject {description: '" + subject.name + "', key: '" + subject.code + "', cost: " + subject.cost + ", term: '" + subject.duration + "' }) ";

                            query += "CREATE (" + course.code + ")-[:CONTAINS]->(" + subject.code + "_" + course.code + ")";
                        }
                    }

                    session.WriteTransactionAsync(async tx =>
                    {
                        var result = tx.RunAsync(query);
                    }).Wait();
                }

                int iterator = 0;
                int next = 0;

                query = string.Empty;

                var country = new List<string>();
                var city = new List<string>();

                var subjectResults = new List<nMarks>();

                foreach (var student in dataset.Students)
                {
                    if (iterator < (dataset.Students.Count - 1))
                        next = iterator + 1;

                    if ((iterator < next && student.idnumber != dataset.Students[next].idnumber) || next == iterator)
                    {
                        query = "CREATE ( p_" + student.register.studentno + " :pupil { " +
                                      "pupilid: " + iterator + "" +
                                      ", studentnum: " + student.register.studentno + "" +
                                      ", idnum: '" + student.idnumber + "'" +
                                      ", title: '" + student.title + "'" +
                                      ", alias: '" + student.surname + "'" +
                                      ", initial: '" + student.name.Substring(0, 1) + "'" +
                                      ", name: '" + student.name + "'" +
                                      ", surname: '" + student.surname + "'" +
                                      ", dob: '" + DateTime.Parse(student.dateofbirth).ToString("yyyy/MM/dd") + "'" +
                                      ", gender: '" + student.gender + "'" +
                                      ", email: '" + student.contact.email + "'" +
                                      ", mobile: '" + student.contact.mobile + "'" +
                              "}) ";

                        query += "CREATE ( pr_" + student.idnumber + ":progress {" +
                                           "studentnum: " + student.register.studentno + "," +
                                           "results: '" + JsonConvert.SerializeObject(subjectResults) + "'" +
                                           "}) ";

                        //country
                        if (!country.Contains(student.address.country.code))
                        {
                            query += "CREATE ( co_" + student.address.country.code + ":country {description: '" + student.address.country + "', key: '" + student.address.country.code + "'}) ";

                            country.Add(student.address.country.code);
                        }

                        //city
                        if (!city.Contains(student.address.city))
                        {
                            query += "CREATE ( ci_" + student.address.city.Replace(" ", "").Replace(".", "") + ":city {description: '" + student.address.city + "', key: '" + student.address.city.Substring(0, 3) + "'}) ";

                            city.Add(student.address.city);
                        }

                        
                        session.WriteTransactionAsync(async tx =>
                        {
                            var result = tx.RunAsync(query);;
                        }).Wait();

                        subjectResults = new List<nMarks>();
                    }

                    subjectResults.Add(new nMarks
                    {
                        subject = new nSubject
                        {
                            //key = dataset.Subjects[] .Values.Where(x => x.) student.transcript.subject,
                            description = student.transcript.subject
                        },
                        score = double.Parse(student.transcript.result),
                        grade = student.transcript.symbol
                    });

                    iterator++;
                }

                iterator = 0;
                next = 0;
                //enrolled in
                foreach (var student in dataset.Students)
                {
                    if (iterator < (dataset.Students.Count - 1))
                        next = iterator + 1;

                    if ((iterator < next && student.idnumber != dataset.Students[next].idnumber))
                    {
                        query = "MATCH (n:pupil), (x:course) WHERE n.idnum = '" + student.idnumber + "' AND x.key = '" + student.register.course.code + "' MERGE (n)-[r:ENROLLED_IN]->(x) ";

                        session.WriteTransactionAsync(async tx =>
                        {
                            var result = tx.RunAsync(query);;
                        }).Wait();
                    }

                    iterator++;
                }

                iterator = 0;
                next = 0;
                //citizen of
                foreach (var student in dataset.Students)
                {
                    if (iterator < (dataset.Students.Count - 1))
                        next = iterator + 1;

                    if ((iterator < next && student.idnumber != dataset.Students[next].idnumber))
                    {
                        query = "MATCH (n:pupil), (x:country) WHERE n.idnum = '" + student.idnumber + "' AND x.key = '" + student.address.country.code + "' MERGE (n)-[co:CITIZEN_OF]->(x)";

                        session.WriteTransactionAsync(async tx =>
                        {
                            var result = tx.RunAsync(query);;
                        }).Wait();
                    }

                    iterator++;
                }

                iterator = 0;
                next = 0;
                //lives in
                foreach (var student in dataset.Students)
                {
                    if (iterator < (dataset.Students.Count - 1))
                        next = iterator + 1;

                    if ((iterator < next && student.idnumber != dataset.Students[next].idnumber))
                    {
                        query = "MATCH (n:pupil), (x:city) WHERE n.idnum = '" + student.idnumber + "' AND x.description = '" + student.address.city + "' MERGE (n)-[li:LIVES_IN]->(x) ";

                        session.WriteTransactionAsync(async tx =>
                        {
                            var result = tx.RunAsync(query);
                        }).Wait();
                    }

                    iterator++;
                }

                //located in
                var location = new List<string>();

                foreach (var student in dataset.Students)
                {
                    if (!location.Contains(student.address.country.code + ":" + student.address.city))
                    {
                        query = "MATCH  (x:city), (n:country) WHERE n.key = '" + student.address.country.code + "' AND x.description = '" + student.address.city + "' MERGE (x)-[li:IS_LOCATED_IN]->(n) ";

                        session.WriteTransactionAsync(async tx =>
                        {
                            var result = tx.RunAsync(query);
                        }).Wait();
                    }

                    location.Add(student.address.country.code + ":" + student.address.city);
                }

                iterator = 0;
                next = 0;
                //transcript
                foreach (var student in dataset.Students)
                {
                    if (iterator < (dataset.Students.Count - 1))
                        next = iterator + 1;

                    if ((iterator < next && student.idnumber != dataset.Students[next].idnumber))
                    {
                        query = "MATCH  (x:pupil), (n:progress) WHERE x.idnum = '" + student.idnumber + "' AND n.studentnum = " + student.register.studentno + " MERGE (x)-[t:TRANSCRIPT]->(n) ";

                        session.WriteTransactionAsync(async tx =>
                        {
                            var result = tx.RunAsync(query);
                        }).Wait();
                    }

                    iterator++;
                }
            }
            catch (Neo4jException ex)
            {
                Console.WriteLine($"Neo4jException - {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neo4jException - {ex.Message}");
                throw;
            }
            finally
            {
                if (_neo4jConnector != null)
                    _neo4jConnector.Disconnect();
            }
        }

        #endregion

        #region Data

        private MockDataset GetMockDataset()
        {
            MockDataset data = new MockDataset
            {
                Courses = new List<MockCourse>(),
                Students = new List<MockPerson>()
            };

            //get course
            var courses = GetCourses();

            //get pupils
            var persons = GetPersons();

            #region Load Course Info

            foreach (var course in courses.OrderBy(x => x.faculty))
            {
                course.facultycode = GenerateThreeRandomLetters();

                foreach (var subject in course.subject)
                    subject.term = GenerateTerm();
            }

            #endregion

            #region Load Student Info

            var numberOfCourses = courses.Count;
            var allocation = Math.Floor(decimal.Parse((persons.Count / numberOfCourses).ToString()));
            var nextCourse = 0;
            var student = 0;

            foreach (var person in persons)
            {
                person.IDNumber = person.IDNumber.Replace("-", "0");
                person.Title = person.Gender == "F" ? "Mrs" : "Mr";

                //get country
                var country = GenerateCountry();

                person.Address = new MockAddress
                {
                    Id = GenerateId(),
                    StreetNo = GenerateIdRange(0, 150),
                    Street = person.Street,
                    StreetAddress = person.StreetAddress,
                    PostalAddress = person.PostalAddress,
                    PostalCode = GenerateIdRange(1000, 4000).ToString(),
                    City = person.City,
                    Location = new MockLocation
                                {
                                    Id = GenerateId(),
                                    Province = GetProvince(country),
                                    CountryCode = GetCountryCode(country),
                                    Country = country
                                }
                };

                if (student > allocation)
                {
                    if(nextCourse + 1 < numberOfCourses)
                        nextCourse++;

                    student = 0;
                }

                //generate data
                var date = DateTime.Now.AddMonths(-GenerateIdRange(0, 48));

                person.Profile = new MockProfile
                {
                    StudentNo = GenerateStudentId(),
                    Username = (person.LastName.Substring(0, 1) + person.FirstName.Replace(" ", "")).ToLower(),
                    Password = GeneratePassword(),
                    IPAddress = person.IPAddress,
                    RegistrationDate = date,
                    GraduatedDate = (date > DateTime.Now ? null : date.AddYears(1)),
                    Course = courses[nextCourse]
                };

                if (person.Profile.Course != null)
                {
                    foreach (var subject in person.Profile.Course.subject)
                    {
                        subject.mark = GenerateMarks();
                        subject.grade = GetGrades(subject.mark);
                    }
                }

                student++;;
            }

            #endregion

            data.Courses = courses;
            data.Students = persons;

            return data;
        }

        private DatabaseModel LoadDataset()
        {
            var dataset = new DatabaseModel
            {
                Faculties = new List<FacultyModel>(),
                Courses = new Dictionary<string, CourseModel>(),
                Subjects = new Dictionary<string, List<SubjectModel>>(),
                Students = new List<ResultsModel>()
            };

            //folder, where a file is created.
            var @base = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\testdata";

            string faculties = "_faculties.txt";
            var fcsv = new StringBuilder();

            string studentprofile = "_students.txt";
            var pcsv = new StringBuilder();

            var count = 0;

            FacultyModel faculty = null;
            CourseModel course = null;
            List<SubjectModel> subjects = null;

            #region faculty load

            if (File.Exists(Path.Combine(@base, faculties)))
            {
                var source = File.ReadAllLines(Path.Combine(@base, faculties));

                var pfaculty = string.Empty;
                var pcourse = string.Empty;

                foreach (var data in source)
                {
                    if (count != 0)
                    {
                        var fields = data.Split("|");

                        if (fields.Length > 1)
                        {
                            if (faculty is null || (faculty is not null && fields[0].ToString() != pfaculty))
                            {
                                faculty = new FacultyModel();
                                faculty.code = fields[0];
                                faculty.name = fields[1];

                                dataset.Faculties.Add(faculty);
                            }

                            if (course is null || (course is not null && fields[2].ToString() != pcourse))
                            {
                                //if(course is not null)
                                //    dataset.Subjects.Add(course.code, subjects);

                                course = new CourseModel();
                                subjects = new List<SubjectModel>();

                                course.code = fields[2];
                                course.name = fields[3];

                                dataset.Courses.Add(faculty.code, course);
                                dataset.Subjects.Add(course.code, subjects);
                            }

                            subjects.Add(new SubjectModel
                            {
                                code = fields[4],
                                name = fields[5],
                                cost = fields[6],
                                duration = fields[7]
                            });
                            

                            pfaculty = fields[0].ToString();
                            pcourse = fields[2].ToString();
                        }
                    }

                    count++;
                }
            }

            #endregion

            #region student load

            if (File.Exists(Path.Combine(@base, studentprofile)))
            {
                var source = File.ReadAllLines(Path.Combine(@base, studentprofile));

                count = 0;

                foreach (var data in source)
                {
                    if (count > 1)
                    {
                        var fields = data.Split("|");

                        if (fields.Length > 1)
                        {
                            var student = new ResultsModel();
                            student.idnumber = fields[0];
                            student.title = fields[1];
                            student.preferredname = fields[2];
                            student.initial = fields[2].Substring(0, 1);
                            student.name = fields[2];
                            student.surname = fields[3];
                            student.dateofbirth = fields[4];
                            student.gender = fields[5];
                            student.language = fields[8];

                            student.contact = new ContactModel();
                            student.contact.email = fields[6];
                            student.contact.mobile = fields[7];

                            student.address = new AddressModel();
                            student.address.streetno = fields[24];
                            student.address.street = fields[25];
                            student.address.postaladdress = fields[25] + "," + fields[26];
                            student.address.postalcode = fields[28];
                            student.address.city = fields[29];
                            student.address.province = fields[31];
                            student.address.country = new CountryModel
                            {
                                code = fields[32],
                                name = fields[33]
                            };
                                
                            student.register = new RegisterModel();
                            student.register.faculty = new FacultyModel
                            {
                                code = fields[15],
                                name = fields[16]
                            };
                            student.register.course = new CourseModel
                            {
                                code = fields[17],
                                name = fields[18]
                            };
                            student.register.subject = new SubjectModel
                            {
                                code = fields[19],
                                name = fields[20],
                                cost = fields[21],
                                duration = fields[22]
                            };
                            student.register.studentno = fields[9];
                            student.register.username = fields[10];
                            student.register.password = fields[11];
                            student.register.type = GenerateEnrollmentType();
                            student.register.date =  fields[13];
                            student.register.completiondate = fields[14];
                            student.register.ipaddress = fields[12];

                            student.transcript = new TranscriptModel { 
                                course = fields[18],
                                subject = fields[35]
                            };
                            student.transcript.result = fields[36];
                            student.transcript.symbol = fields[37];

                            dataset.Students.Add(student);
                        }
                    }

                    count++;
                }
            }

            #endregion

            return dataset;
        }

        #endregion

        #region Helpers - Data generaors

        private List<MockCourse> GetCourses()
        {
            List<MockCourse> response = null;
            //
            //Open the json file using a stream reader.
            using (var sr = new StreamReader(@"Repos/_data/00_template_faculty_course_subject.json"))
            {
                // Read the stream as a string, and write the string to the console.
                var data = sr.ReadToEnd();

                if (!string.IsNullOrEmpty(data))
                {
                    response = JsonConvert.DeserializeObject<List<MockCourse>>(data);
                }
            }

            return response;
        }

        private List<MockPerson> GetPersons()
        {
            List<MockPerson> response = null;

            //Open the json file using a stream reader.
            using (var sr = new StreamReader(@"Repos/_data/01_template_student_add_loc.json"))
            {
                // Read the stream as a string, and write the string to the console.
                var data = sr.ReadToEnd();

                if (!string.IsNullOrEmpty(data))
                {
                    response = JsonConvert.DeserializeObject<List<MockPerson>>(data);
                }
            }

            return response;
        }

        private string GenerateThreeRandomLetters()
        {
            Random rnd = new Random();
            var letters = string.Empty;
            char letter;
            int asciiIndex = 0;

            for (var i = 0; i < 2; i++)
            {
                asciiIndex = rnd.Next(65, 91); //ASCII character codes 65-90

                letter = Convert.ToChar(asciiIndex); //produces any char A-Z

                letters += letter;
            }

            return letters;
        }

        private int GenerateId()
        {
            return new Random().Next(10000, 9999999);
        }

        private int GenerateStudentId()
        {
            return new Random().Next(100000000, 999999990);
        }

        private int GenerateTerm()
        {
            return new Random().Next(3, 12);
        }

        private string GeneratePassword()
        {
            int passwordSize = 10;
            char[] _password = new char[passwordSize];
            string charSet = ""; // Initialise to blank
            System.Random _random = new Random();
            int counter;

            // Build up the character set to choose from
            charSet += "abcdefghijklmnopqursuvwxyz";

            charSet += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            charSet += "123456789";

            charSet += @"!@£$%^&*()#€";

            for (counter = 0; counter < passwordSize; counter++)
            {
                _password[counter] = charSet[_random.Next(charSet.Length - 1)];
            }

            return String.Join(null, _password);
        }

        private int GenerateIdRange(int start, int end)
        {
            return new Random().Next(start, end);
        }

        private double GenerateMarks()
        {
            return new Random().Next(0, 100);
        }

        private string GetProvince(string country)
        {
            string province = string.Empty;

            switch (country)
            {
                case "South Africa":
                    province = GenerateSAProvince();
                    break;
                case "Angola":
                    province = GenerateAngolaProvince();
                    break;
                case "Nigeria":
                    province = GenerateNigeriaProvince();
                    break;
                case "Namibia":
                    province = GenerateNamibiaProvince();
                    break;
                case "Botswana":
                    province = GenerateBotswanaProvince();
                    break;
                case "Egypt":
                    province = GenerateEgyptProvince();
                    break;
                default:
                    province = GenerateTunisiaProvince();
                    break;
            }

            return province;
        }

        private string GenerateSAProvince()
        {
            var list = new List<string> {
                "Eastern Cape",
                "Free State",
                "Gauteng",
                "KwaZulu-Natal",
                "Limpopo",
                "Mpumalanga",
                "Northern Cape",
                "North West",
                "Western Cape"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateAngolaProvince()
        {
            var list = new List<string> {
                "Bengo",
                "Benguela",
                "Bié",
                "Cabinda",
                "Cuando Cubango",
                "Cuanza Norte",
                "Cuanza Sul",
                "Cunene",
                "Huambo",
                "Huíla",
                "Luanda",
                "Lunda Norte",
                "Lunda Sul",
                "Malanje",
                "Moxico",
                "Namibe",
                "Uíge",
                "Zaire"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateNigeriaProvince()
        {
            var list = new List<string> {
                "Bauchi",
                "Bida",
                "Bornu",
                "Kabba",
                "Kontagora",
                "Lower Benue or Nassarawa",
                "Illorin",
                "Muri",
                "Sokoto",
                "Upper Bema",
                "Zaria"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateNamibiaProvince()
        {
            var list = new List<string> {
                "Caprivi",
                "Erongo",
                "Hardap",
                "Karas",
                "Kavango West",
                "Kavango East",
                "Khomas",
                "Kunene",
                "Ohangwena",
                "Omaheke",
                "Omusati",
                "Oshana",
                "Oshikoto",
                "Otjozondjupa"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateBotswanaProvince()
        {
            var list = new List<string> {
                "Central",
                "Ghanzi",
                "Kgalagadi",
                "Kgatleng",
                "Kweneng",
                "North East",
                "North West",
                "South East",
                "Southern"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateEgyptProvince()
        {
            var list = new List<string> {
               "Alexandria Governorate",
                "Aswan Governorate",
                "Asyut Governorate",
                "Beheira Governorate",
                "Beni Suef Governorate",
                "Cairo Governorate",
                "Dakahlia Governorate",
                "Damietta Governorate",
                "Faiyum Governorate",
                "Gharbia Governorate",
                "Giza Governorate",
                "Ismailia Governorate",
                "Kafr El Sheikh Governorate",
                "Luxor Governorate",
                "Matruh Governorate",
                "Minya Governorate",
                "Monufia Governorate",
                "New Valley Governorate",
                "North Sinai Governorate",
                "Port Said Governorate[5]",
                "Qalyubia Governorate",
                "Qena Governorate",
                "Red Sea Governorate",
                "Sharqia Governorate",
                "Sohag Governorate",
                "South Sinai Governorate",
                "Suez Governorate"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateTunisiaProvince()
        {
            var list = new List<string> {
                "Ariana",
                "Béja",
                "Ben Arous",
                "Bizerte",
                "Gabès",
                "Gafsa",
                "Jendouba",
                "Kairouan",
                "Kasserine",
                "Kebili",
                "Kef",
                "Mahdia",
                "Manouba",
                "Medenine",
                "Monastir",
                "Nabeul",
                "Sfax",
                "Sidi Bouzid",
                "Siliana",
                "Sousse",
                "Tataouine",
                "Tozeur",
                "Tunis",
                "Zaghouan"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateCountry()
        {
            var list = new List<string> { "South Africa", "Angola", "Nigeria", "Namibia", "Botswana", "Egypt", "Tunisia" };

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GetCountryCode(string country)
        {
            string code = string.Empty;

            switch (country)
            {
                case "South Africa":
                    code = "ZA";
                    break;
                case "Angola":
                    code = "AO";
                    break;
                case "Nigeria":
                    code = "NG";
                    break;
                case "Namibia":
                    code = "NA";
                    break;
                case "Botswana":
                    code = "BW";
                    break;
                case "Egypt":
                    code = "EG";
                    break;
                default:
                    code = "TN";
                    break;
            }

            return code;
        }

        private string GetGrades(double mark)
        {
            if (mark >= 80)
            {
                return "A";
            }
            else if (mark >= 70)
            {
                return "B";
            }
            else if (mark >= 60)
            {
                return "C";
            }
            else if (mark >= 50)
            {
                return "D";
            }
            else if (mark >= 40)
            {
                return "E";
            }
            else if (mark >= 30)
            {
                return "F";
            }
            else
            {
                return "G";
            }
        }

        private string GenerateEnrollmentType()
        {
            var random =  new Random().Next(0, 100);

            if(random < 20)
                return "Part-Time";
            else
                return "Full-Time";
        }

        #endregion
    }
}
