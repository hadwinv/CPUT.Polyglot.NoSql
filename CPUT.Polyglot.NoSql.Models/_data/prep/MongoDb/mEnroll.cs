namespace CPUT.Polyglot.NoSql.Models._data.prep.MongoDb
{
    public class mEnroll
    {
        public mFaculty faculty { get; set; }
        public mCourse course { get; set; }
        public List<mSubject> subject { get; set; }
        public string enrollment_type { get; set; }
        public DateTime enrollment_date { get; set; }
    }
}
