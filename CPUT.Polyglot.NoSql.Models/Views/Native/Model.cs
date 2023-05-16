namespace CPUT.Polyglot.NoSql.Models.Views.Native
{
    public class Model
    {
        public string Name { get; set; }

        public List<Properties> Properties { get; set; }

        public string Type { get; set; }

        public List<Relations> Relations { get; set; }
    }
}
