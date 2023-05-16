using CPUT.Polyglot.NoSql.Models.Views.Native;

namespace CPUT.Polyglot.NoSql.Models.Views
{
    public class NSchema
    {
        public string Name { get; set; }
        public string Storage { get; set; }
        public List<Model> Model { get; set; }
    }
}
