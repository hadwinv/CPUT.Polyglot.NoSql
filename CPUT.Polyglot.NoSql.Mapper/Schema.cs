﻿using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Interface;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Models.Mapper;
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

        public List<MappedSource> Mapper()
        {
            //get from cache;
            var schema = (List<MappedSource>)_cache.GetInMemory("binder");

            if (schema == null)
            {
                var data = Utils.ReadTemplate(@"Templates/Configs/MapperTemplate.json");

                if (!string.IsNullOrEmpty(data))
                {
                    schema = JsonConvert.DeserializeObject<List<MappedSource>>(data);

                    _cache.AddToInMemory("binder", schema);
                }
            }

            return schema;
        }

        public List<USchema> Global()
        {
            //get from cache;
            var schema = (List<USchema>)_cache.GetInMemory("global");

            if (schema == null)
            {
                var data = Utils.ReadTemplate(@"Templates/Configs/GlobalTemplate.json");

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
                var data = Utils.ReadTemplate(@"Templates/Configs/Native/KeyValueTemplate.json");

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
                var data = Utils.ReadTemplate(@"Templates/Configs/Native/ColumnarTemplate.json");

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
                var data = Utils.ReadTemplate(@"Templates/Configs/Native/DocumentTemplate.json");

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
                var data = Utils.ReadTemplate(@"Templates/Configs/Native/GraphTemplate.json");

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
