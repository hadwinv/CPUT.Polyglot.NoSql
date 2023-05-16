using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Translator;
using System;
using System.Collections.Generic;

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
            try
            {
                if (construct.Query != null)
                {
                    _session.Connect().Execute(construct.Query);

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
            string query = string.Empty;
            int count = 0;
            try
            {
                //call to create structure
                CreateDocumentSchema();

                //data insert
                foreach (var student in dataset[0].Students)
                {
                    //
                    query = @"INSERT INTO cput.student (idno, firstname, lastname, dob, email, cellnumber, homenumber, address, registered)  
                                    VALUES( '" + student.IdNumber + "', " +
                                        "'" + student.Name + "'," +
                                        "'" + student.Surname + "'," +
                                        "'" + student.DOB + "'," +
                                        "'" + student.Email + "'," +
                                        "'" + student.MobileNo + "'," +
                                        "'" + student.HomeNo + "'," +
                                        @"{
                                                streetno: '" + student.Address.StreetNo + "'," +
                                               "streetname: '" + student.Address.Street + "'," +
                                               "postalcode: ''," +
                                               "suburb: ''," +
                                               "city: '" + student.Address.City + "'" +
                                           "}," +
                                        @"{
                                                studentno : '" + student.Profile.StudentNo + "'," +
                                                "coursename : '" + student.Profile.Course.Description + "'," +
                                                "subject : {";

                    count = 0;

                    foreach (var subject in student.Profile.Course.Subjects)
                    {
                        query = query + "{" +
                                           "code: '" + subject.Code + "', " +
                                           "name: '" + subject.Description + "'";

                        if (count == student.Profile.Course.Subjects.Count - 1)
                            query = query + "}";
                        else
                            query = query + "},";

                        count++;
                    }
                    query = query + "}}";
                    
                    query = query + @");";

                    _session.Connect().Execute(query);

                    //grades
                    foreach (var mark in student.Marks)
                    {
                        query = @"INSERT INTO cput.grades (id, idno, subject, marks, symbol) 
                             VALUES(" + mark.Id + "," +
                                        "'" + student.IdNumber + "', " +
                                        "{" +
                                        " code : '" + mark.SubjectCode + "'," +
                                        " name : '" + mark.Subject + "'" +
                                        "}," +
                                         mark.Score + "," +
                                        "'" + mark.Grade + "')";

                        _session.Connect().Execute(query);
                    }
                }

                _session.Connect().Execute("CREATE INDEX firstname_idx ON cput.student(firstname);");
                _session.Connect().Execute("CREATE INDEX lastname_idx ON cput.student(lastname);");
                _session.Connect().Execute("CREATE INDEX idno_idx ON cput.grades(idno);");
                _session.Connect().Execute("CREATE INDEX marks_idx ON cput.grades(marks);");

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
            _session.Connect().Execute("DROP TABLE IF EXISTS cput.grades;");

            
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.address;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.registered;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.subject;");

            string subject = @"CREATE TYPE IF NOT EXISTS cput.subject( 
                    code varchar,
                    name varchar);";

            _session.Connect().Execute(subject);

            string address = @"CREATE TYPE IF NOT EXISTS cput.address( 
                    streetno varchar,
                    streetname varchar,
                    postalcode varchar,
                    suburb varchar,
                    city varchar);";

            _session.Connect().Execute(address);

            string registered = @"CREATE TYPE IF NOT EXISTS cput.registered( 
                    studentno varchar,
                    coursename varchar,
                    subject frozen<set<frozen<subject>>>);";

            _session.Connect().Execute(registered);
            
            string student = @"CREATE TABLE IF NOT EXISTS cput.student(
                    idno text, 
                    firstname varchar,
                    lastname varchar,
                    dob varchar,
                    email varchar,
                    cellnumber varchar,
                    homenumber varchar,
                    address frozen<address>,
                    registered frozen<registered>,
                    PRIMARY KEY (idno)
                    ); ";

            _session.Connect().Execute(student);

            string grades = @"CREATE TABLE IF NOT EXISTS cput.grades(
                    id int, 
                    idno text, 
                    subject frozen<subject>,
                    marks decimal,
                    symbol varchar,
                    PRIMARY KEY (id)
                    ); ";

            _session.Connect().Execute(grades);
        }

        #endregion

    }
}
