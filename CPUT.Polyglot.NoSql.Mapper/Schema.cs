using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Interface;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Models.Views;
using Newtonsoft.Json;

namespace CPUT.Polyglot.NoSql.Mapper
{
    public class Schema : ISchema
    {
        private ICache _cache;

        public Schema(ICache cache)
        {
            _cache = cache;
        }

        public List<USchema> UnifiedView()
        {
            //get from cache;
            var schema = (List<USchema>)_cache.GetInMemory("global");

            if (schema == null)
            {
                var data = Utils.ReadTemplate(@"_schemas/_unifiedview.json");

                if (!string.IsNullOrEmpty(data))
                {
                    schema = JsonConvert.DeserializeObject<List<USchema>>(data);

                    _cache.AddToInMemory("global", schema);
                }
            }

            return schema;
        }

        public List<NSchema> KeyValue()
        {
            //get from cache;
            var schema = (List<NSchema>)_cache.GetInMemory("keyvalue");

            if (schema == null)
            {
                var data = Utils.ReadTemplate(@"_schemas/_native/_keyvalue.json");

                if (!string.IsNullOrEmpty(data))
                {
                    schema = JsonConvert.DeserializeObject<List<NSchema>>(data);

                    _cache.AddToInMemory("keyvalue", schema);
                }
            }

            return schema;
        }

        public List<NSchema> Columnar()
        {
            //get from cache;
            var schema = (List<NSchema>)_cache.GetInMemory("columnar");

            if (schema == null)
            {
                var data = Utils.ReadTemplate(@"_schemas/_native/_columnar.json");

                if (!string.IsNullOrEmpty(data))
                {
                    schema = JsonConvert.DeserializeObject<List<NSchema>>(data);

                    _cache.AddToInMemory("columnar", schema);
                }
            }

            return schema;
        }

        public List<NSchema> Document()
        {
            //get from cache;
            var schema = (List<NSchema>)_cache.GetInMemory("document");

            if (schema == null)
            {
                var data = Utils.ReadTemplate(@"_schemas/_native/_document.json");

                if (!string.IsNullOrEmpty(data))
                {
                    schema = JsonConvert.DeserializeObject<List<NSchema>>(data);

                    _cache.AddToInMemory("document", schema);
                }
            }

            return schema;
        }

        public List<NSchema> Graph()
        {
            //get from cache;
            var schema = (List<NSchema>)_cache.GetInMemory("graph");

            if (schema == null)
            {
                var data = Utils.ReadTemplate(@"_schemas/_native/_graph.json");

                if (!string.IsNullOrEmpty(data))
                {
                    schema = JsonConvert.DeserializeObject<List<NSchema>>(data);

                    _cache.AddToInMemory("graph", schema);
                }
            }

            return schema;
        }
    }
}
