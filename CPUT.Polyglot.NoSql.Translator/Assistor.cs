using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using static System.Net.Mime.MediaTypeNames;

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

        public static string UnwindProperty(Model model, int target)
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

                        if (next?.Type == "collection" || next?.Type == "table")
                            return name;
                        else
                        {
                            var properties = UnwindProperty(parent, target);

                            return !string.IsNullOrEmpty(properties) ? properties + "." + name : name;
                        }
                    }
                }
            }

            return name;
        }

        //public static string UnwindPropertyTest(string path, int target)
        //{
        //    var name = string.Empty;

        //    if (child?.Type == "json")
        //    {
        //        if (parent != null)
        //        {
        //            var property = parent.Properties.SingleOrDefault(x => x.Type == child.Name);

        //            if (property != null)
        //            {
        //                name = property.Property;

        //                var next = NSchema[target].SelectMany(x => x.Model.Where(x => x.Properties.Exists(x => x.Type == child.Name))).FirstOrDefault();

        //                if (next?.Type == "collection" || next?.Type == "table")
        //                    return name;
        //                else
        //                {
        //                    var properties = UnwindPropertyTest(parent, null, target);

        //                    return !string.IsNullOrEmpty(properties) ? properties + "." + name : name;
        //                }
        //            }
        //        }
        //    }

        //    return name;
        //}


        //public static string UnwindChild(Model model, string @base, int target)
        //{
        //    if (model?.Type == "json")
        //    {
        //        var parent = NSchema[target].SelectMany(x => x.Model.Where(x => x.Name == @base && x.Properties.Exists(x => x.Type == model.Name))).FirstOrDefault();

        //        if (parent != null && parent?.Type == "json")
        //        {
        //            var property = UnwindChild(parent, @base, target);

        //            return !string.IsNullOrEmpty(property) ? property + "." + parent.Name : parent.Name;
        //        }
        //    }

        //    return string.Empty;
        //}


        public static void Add(int target, List<NSchema> schemas)
        {
            if (_nSchema == null)
                _nSchema = new Dictionary<int, List<NSchema>>();

            if(!_nSchema.ContainsKey(target))
                _nSchema.Add(target, schemas);
        }
    }
}
