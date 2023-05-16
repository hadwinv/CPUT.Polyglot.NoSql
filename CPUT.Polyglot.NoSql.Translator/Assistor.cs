using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;

namespace CPUT.Polyglot.NoSql.Translator
{
    public static class Assistor
    {
        public static List<USchema> USchema { get; set; }

        public static List<NSchema> NSchema { get; set; }

        public static string UnwindPropertyName(Model model)
        {
            var name = string.Empty;

            if (model?.Type == "json")
            {
                var parent = NSchema.SelectMany(x => x.Model.Where(x => x.Properties.Exists(x => x.Type == model.Name))).FirstOrDefault();

                if (parent != null)
                {
                    var property = parent.Properties.SingleOrDefault(x => x.Type == model.Name);

                    if (property != null)
                    {
                        name = property.Property;

                        var next = NSchema.SelectMany(x => x.Model.Where(x => x.Properties.Exists(x => x.Type == model.Name))).FirstOrDefault();

                        if (next?.Type == "collection")
                            return name;
                        else
                            return UnwindPropertyName(parent) + "." + name;
                    }
                }
            }

            return name;
        }
    }
}
