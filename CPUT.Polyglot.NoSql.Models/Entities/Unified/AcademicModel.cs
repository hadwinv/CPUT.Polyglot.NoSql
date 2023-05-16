namespace CPUT.Polyglot.NoSql.Models.Entities.Unified
{
    public class AcademicModel
    {
        public SubjectModel Subject { get; set; }
        public double Marks { get; set; }
        public double Percentage { get; set; }
        public string GradedSymbol { get; set; }
    }
}
