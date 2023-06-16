using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Translator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUT.Polyglot.NoSql.DataStores.Repos.Columnar
{
    public class CassandraRepo : ICassandraRepo
    {
        private ICassandraBridge _session;

        public CassandraRepo(ICassandraBridge session)
        {
            _session = session;
        }

        public Models.Result Execute(Constructs construct)
        {
            Models.Result result = null;

            try
            {
                if (construct.Query != null)
                {
                    var response = _session.Connect().Execute(construct.Query);

                    result = new Models.Result
                    {
                        Data = response,
                        Message = "OK",
                        Success = true
                    };
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
            string query = string.Empty;
            int count = 0;
            try
            {
                //call to create structure
                CreateDocumentSchema();

                //data insert
                foreach (var student in dataset[0].Students)
                {
                    var studentid = 1000;

                    foreach (var mark in student.Marks)
                    {
                        var faculty = dataset[0].Faculties.Where(x => x.Courses.Exists(x => x.Description == student.Profile.Course.Description)).SingleOrDefault();
                        //, 
                        query = @"INSERT INTO cput.student (uniqueid, idno, studentno, title, aka, initials, firstname, lastname, dob, genderid, email, cellno, address, registered, grades )  
                                    VALUES( " +
                                        "'" + studentid + "', " +
                                        "'" + student.IdNumber + "', " +
                                        "'" + student.Profile.StudentNo + "'," +
                                        "'" + student.Title + "'," +
                                        "'" + student.Name + "'," +
                                        "'" + student.Name.Substring(0, 1) + "'," +
                                        "'" + student.Name + "'," +
                                        "'" + student.Surname + "'," +
                                        "'" + student.DOB + "'," +
                                        "'" + student.Gender + "'," +
                                        "'" + student.Email + "'," +
                                        "'" + student.MobileNo + "'," +
                                        @"{
                                            streetno: " + student.Address.StreetNo + "," +
                                                "streetname: '" + student.Address.Street + "'," +
                                                "city: '" + student.Address.City + "'," +
                                                "postalcode: '" + student.Address.PostalCode + "'," +
                                                "country: '" + student.Address.Location.Country + "'" +
                                        "}," +
                                        @"{
                                            faculty : '" + faculty.Description + "'," +
                                                "course : '" + student.Profile.Course.Description + "'," +
                                                "registerdate : '" + DateTime.Now.ToString("yyyy-MM-dd") + "'," +
                                                "subject : {";

                        count = 0;

                        foreach (var item in student.Profile.Course.Subjects)
                        {
                            query = query + "{" +
                                               "descr: '" + item.Description + "', " +
                                               "price: " + item.Price + "," +
                                               "period: " + item.Duration + "";

                            if (count == student.Profile.Course.Subjects.Count - 1)
                                query = query + "}";
                            else
                                query = query + "},";

                            count++;
                        }
                        query = query + "}}";

                        //grades
                        query = query + ",{";

                        var subject = student.Profile.Course.Subjects
                            .Where(x => x.Code == mark.SubjectCode)
                            .SingleOrDefault();

                        query = query +
                            "subject: {" +
                                "descr: '" + subject.Description + "', " +
                                 "price: " + subject.Price + "," +
                                  "period: " + subject.Duration + "" +
                            "}," +
                            "marks: " + mark.Score + "," +
                            "symbol: '" + mark.Grade + "'";

                        query = query + "}";

                        query = query + @");";

                        _session.Connect().Execute(query);

                        studentid++;
                    }
                  
                }

                _session.Connect().Execute("CREATE INDEX idno_idx ON cput.student(idno);");
                _session.Connect().Execute("CREATE INDEX studentno_idx ON cput.student(studentno);");

                _session.Connect().Execute("CREATE INDEX genderid_idx ON cput.student(genderid);");

                _session.Connect().Execute("CREATE INDEX firstname_idx ON cput.student(firstname);");
                _session.Connect().Execute("CREATE INDEX lastname_idx ON cput.student(lastname);");
                    
                _session.Connect().Execute("CREATE INDEX address_idx ON cput.student(address);");
                _session.Connect().Execute("CREATE INDEX registered_idx ON cput.student(registered);");
                
                _session.Connect().Execute("CREATE INDEX grades_idx ON cput.student(grades);");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
                throw;
            }
            finally
            {
                if (_session != null)
                    _session.Disconnect();
            }
        }

        private void CreateDocumentSchema()
        {
            _session.Connect().Execute("CREATE KEYSPACE IF NOT EXISTS cput WITH REPLICATION = { 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 };");

            _session.Connect().Execute("DROP TABLE IF EXISTS cput.student;");

            
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.address;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.registered;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.grades;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.subject;");

            string subject = @"CREATE TYPE IF NOT EXISTS cput.subject( 
                    descr varchar,
                    price decimal,
                    period int);";

            _session.Connect().Execute(subject);

            string address = @"CREATE TYPE IF NOT EXISTS cput.address( 
                    streetno int,
                    streetname varchar,
                    city varchar,
                    postalcode varchar,
                    country varchar);";

            _session.Connect().Execute(address);

            string registered = @"CREATE TYPE IF NOT EXISTS cput.registered( 
                    faculty varchar,
                    course varchar,
                    subject frozen<set<subject>>,
                    registerdate timestamp);";

            _session.Connect().Execute(registered);

            string grades = @"CREATE TYPE IF NOT EXISTS cput.grades(
                    subject frozen<subject>,
                    marks decimal,
                    symbol varchar
                    ); ";

            _session.Connect().Execute(grades);

            string student = @"CREATE TABLE IF NOT EXISTS cput.student(
                    uniqueid varchar, 
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
                    PRIMARY KEY (uniqueid)
                    ); ";

            _session.Connect().Execute(student);

           
        }

        #endregion

    }
}
