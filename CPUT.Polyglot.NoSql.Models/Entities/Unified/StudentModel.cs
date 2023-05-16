namespace CPUT.Polyglot.NoSql.Models.Entities.Unified
{
    public class StudentModel
    {
        public string Title { get; set; }
        public string Initial { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string IdNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public AddressModel Address { get; set; }
        public ContactModel Contact { get; set; }
        public RegistrationModel Registration { get; set; }
        public AcademicModel Transcript { get; set; }
    }
}