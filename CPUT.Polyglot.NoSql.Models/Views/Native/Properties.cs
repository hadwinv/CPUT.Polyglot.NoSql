using CPUT.Polyglot.NoSql.Models.Views.Shared;

namespace CPUT.Polyglot.NoSql.Models.Views.Native
{
    public class Properties
    {
        public string Property { get; set; }

        public string Type { get; set; }

        public bool Key { get; set; }

        public bool Indexed { get; set; }

        public string Metadata { get; set; }
    }
}
