using CPUT.Polyglot.NoSql.Models.Views.Shared;

namespace CPUT.Polyglot.NoSql.Models.Views.Native
{
    public class Properties
    {
        public bool Key { get; set; }

        public string Property { get; set; }

        public string Type { get; set; }

        public List<Link> Link { get; set; }
    }
}
