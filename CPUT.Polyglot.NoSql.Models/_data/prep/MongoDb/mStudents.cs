using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;

namespace CPUT.Polyglot.NoSql.Models._data.prep.MongoDb
{
    [BsonIgnoreExtraElements]
    public class mStudents
    {
        [BsonElement("student_id")]
        public string student_id { get; set; }

        [BsonElement("student_no")]
        public string student_no { get; set; }

        [BsonElement("id_number")]
        public string id_number { get; set; }

        [BsonElement("title")]
        public string title { get; set; }

        [BsonElement("init")]
        public string init { get; set; }

        [BsonElement("name")]
        public string name { get; set; }

        [BsonElement("surname")]
        public string surname { get; set; }

        [BsonElement("date_of_birth")]
        public DateTime date_of_birth { get; set; }

        [BsonElement("gender_identity")]
        public string gender_identity { get; set; }

        [BsonElement("contact")]
        public mContact contact { get; set; }

        [BsonElement("address")]
        public mAddress address { get; set; }
        
        [BsonElement("enroll")]
        public mEnroll enroll { get; set; }
    }
}
