using CPUT.Polyglot.NoSql.Models.Views.Shared;

namespace CPUT.Polyglot.NoSql.Models.Views.Unified
{
    public class Resources
    {
        public string Property { get; set; }

        public string Type { get; set; }

        public string Metadata { get; set; }

        public List<Link> Link { get; set; }
    }
}
