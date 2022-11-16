namespace CPUT.Polyglot.NoSql.Models.Translator
{
    public class Schemas
    {
        public List<Validators> ValidatorResults { get; set; }

        public Schemas()
        {
            ValidatorResults = new List<Validators>();
        }
    }
}
