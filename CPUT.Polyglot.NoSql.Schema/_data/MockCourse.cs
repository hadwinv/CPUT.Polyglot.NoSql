namespace CPUT.Polyglot.NoSql.Schema._data
{
    public class MockCourse
	{
		public string code { get; set; }
		public string name { get; set; }
		public string faculty { get; set; }
		public List<MockSubject> subject { get; set; }
	}
}
