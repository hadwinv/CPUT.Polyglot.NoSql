using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;

namespace CPUT.Polyglot.NoSql.Translator
{
    public static class Assistor
    {
        public static List<USchema> USchema { get; set; }

        private static Dictionary<int, List<NSchema>> _nSchema { get; set; }
        public static Dictionary<int, List<NSchema>> NSchema 
        {
            get
            {
                return _nSchema;
            }
        }

        public static string UnwindPropertyName(Model model, int target)
        {
            var name = string.Empty;

            if (model?.Type == "json")
            {
                var parent = NSchema[target].SelectMany(x => x.Model.Where(x => x.Properties.Exists(x => x.Type == model.Name))).FirstOrDefault();

                if (parent != null)
                {
                    var property = parent.Properties.SingleOrDefault(x => x.Type == model.Name);

                    if (property != null)
                    {
                        name = property.Property;

                        var next = NSchema[target].SelectMany(x => x.Model.Where(x => x.Properties.Exists(x => x.Type == model.Name))).FirstOrDefault();

                        if (next?.Type == "collection")
                            return name;
                        else
                            return UnwindPropertyName(parent, target) + "." + name;
                    }
                }
            }

            return name;
        }

        public static void Add(int target, List<NSchema> schemas)
        {
            if (_nSchema == null)
                _nSchema = new Dictionary<int, List<NSchema>>();

            if(!_nSchema.ContainsKey(target))
                _nSchema.Add(target, schemas);
        }
    }
}
