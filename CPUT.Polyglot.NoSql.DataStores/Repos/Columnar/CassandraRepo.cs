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
                     query = @"INSERT INTO cput.student (idno, studentno, person, addressdetail, enrollment, academicresult)  
                                    VALUES( '" + student.IdNumber + "', " +
                                        "'" + student.Profile.StudentNo + "'," +
                                        @"{'" + student.Profile.StudentNo + @"': {
                                                firstname: '" + student.Name + "'," +
                                                "lastname: '" + student.Surname + "'," +
                                                "dob: '" + student.DOB + "'," +
                                                "email: '" + student.Email + "'," +
                                                "cellnumber: '" + student.MobileNo + "'," +
                                                "homenumber: '" + student.HomeNo + "'}" +
                                         "}," +
                                        @"{'" + student.Profile.StudentNo + @"' : {
                                                streetno: '" + student.Address.StreetNo + "'," +
                                                "streetname: '" + student.Address.Street + "'," +
                                                "postalcode: ''," +
                                                "suburb: ''," +
                                                "city: '" + student.Address.City + "'}" +
                                            "}," +
                                         @"{'" + student.Profile.StudentNo + @"': {
                                                course : '" + student.Profile.Course.Description + "'," +
                                                "subject: { ";

                    count = 0;

                    foreach (var subject in student.Profile.Course.Subjects)
                    {
                        query = query + "'" + subject.Code + "' : {" +
                                                                "code: '" + subject.Code + "', " +
                                                                "name: '" + subject.Description + "'";

                        if (count == student.Profile.Course.Subjects.Count - 1)
                            query = query + "}";
                        else
                            query = query + "},";

                        count++;
                    }

                    query = query + @"}}}, {";

                    count = 0;
                    foreach (var mark in student.Marks)
                    {
                        query = query + "'" + mark.SubjectCode + "':{ " +
                                            "subject: '" + mark.Subject + "'," +
                                            "marks: " + mark.Score + "," +
                                            "grade: '" + mark.Grade + "'";

                        if (count == student.Marks.Count - 1)
                            query = query + "}";
                        else
                            query = query + "},";

                        count++;
                    }

                    query = query + @"});";

                    _session.Connect().Execute(query);
                }

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
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.person;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.addressdetail;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.enrollment;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.subject;");
            _session.Connect().Execute("DROP TYPE IF EXISTS cput.academicresult;");

            string personType = @"CREATE TYPE IF NOT EXISTS cput.person( 
                    firstname varchar,
                    lastname varchar,
                    dob varchar,
                    email varchar,
                    cellnumber varchar,
                    homenumber varchar);";

            _session.Connect().Execute(personType);

            string addressType = @"CREATE TYPE IF NOT EXISTS cput.addressdetail( 
                    streetno varchar,
                    streetname varchar,
                    postalcode varchar,
                    suburb varchar,
                    city varchar);";

            _session.Connect().Execute(addressType);

            string subjectType = @"CREATE TYPE IF NOT EXISTS cput.subject( 
                    code varchar,
                    name varchar);";

            _session.Connect().Execute(subjectType);

            string enrollmentType = @"CREATE TYPE IF NOT EXISTS cput.enrollment( 
                    course varchar,
                    subject map<text, frozen<subject>>);";

            _session.Connect().Execute(enrollmentType);

            string academicresultType = @"CREATE TYPE IF NOT EXISTS cput.academicresult( 
                    subject varchar,
                    marks decimal,
                    grade varchar);";

            _session.Connect().Execute(academicresultType);

            string query = "CREATE TABLE IF NOT EXISTS cput.student(" +
                    "idno text PRIMARY KEY, "
                    + "studentno text, "
                    + "person map<text, frozen<person>>,"
                    + "addressdetail map<text, frozen<addressdetail>>,"
                    + "enrollment map<text, frozen<enrollment>>,"
                    + "academicresult map<text, frozen<academicresult>>"
                    + "); ";

            _session.Connect().Execute(query);
        }

        #endregion

    }
}
