using Cassandra;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using CPUT.Polyglot.NoSql.Models.Views.Bindings;
using CPUT.Polyglot.NoSql.Models.Views.Shared;
using CPUT.Polyglot.NoSql.Models.Views.Unified;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using CPUT.Polyglot.NoSql.Translator;
using DnsClient.Protocol;
using MongoDB.Bson;
using MongoDB.Driver;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Superpower.Parsers;

namespace CPUT.Polyglot.NoSql.Mapper.ViewMap
{
    public static class ModelBuilder
    {
        public static List<ResultsModel> Create(Codex codex, RowSet records)
        {
            var results = new List<ResultsModel>();

            var type = typeof(ResultsModel);

            if (codex.DataModel != null)
            {
                foreach (var expr in codex.DataModel.Value)
                {
                    if (expr is DataExpr)
                    {
                        //get primary model for selection base
                        var data = (DataExpr)expr;

                        var @base = Assistor.USchema.Single(x => x.View.Name == data.Value);

                        foreach (var row in records)
                        {
                            var model = new ResultsModel();

                            foreach (var view in codex.PropertyModel[0].Views)
                            {
                                if (view.Value.Name.IndexOf('.') > -1)
                                {
                                    var parts = view.Value.Name.Split('.');

                                    Type? @class = null;

                                    object? instance = null;
                                    object? pinstance = null;
                                    Resources resource = null;
                                    Resources presource = null;
                                    var partIterator = 0;

                                    foreach (var part in parts)
                                    {
                                        if (partIterator == 0)
                                            resource = @base.View.Resources.Single(x => x.Property == part);
                                        else
                                        {
                                            var schema = Assistor.USchema.Single(x => x.View.Name == presource?.Property);

                                            resource = schema.View.Resources.Single(x => x.Property == part);
                                        }

                                        if (resource?.Metadata == "class")
                                        {
                                            @class = GetConcreteClassType(resource.Type);

                                            if (partIterator > 0)
                                                instance = pinstance.GetType().GetProperty(resource.Property).GetValue(pinstance, null);
                                            else
                                                instance = model.GetType().GetProperty(resource.Property).GetValue(model, null);
                                        }
                                        else
                                        {
                                            var propertyInfo = @class?.GetProperty(resource.Property);

                                            if (propertyInfo != null)
                                            {
                                                var link = SearchAndFindPropertyLink(presource.Property, resource.Property, "cassandra");

                                                if (link != null)
                                                {
                                                    if(!string.IsNullOrEmpty(view.Key.Alias))
                                                    {
                                                        if (row.GetValue<object>(view.Key.Alias) is not null)
                                                            propertyInfo.SetValue(instance, row.GetValue<object>(view.Key.Alias).ToString(), null);
                                                    }
                                                    else
                                                    {
                                                        if (row.GetValue<object>(view.Key.Name) is not null)
                                                            propertyInfo.SetValue(instance, row.GetValue<object>(view.Key.Name).ToString(), null);
                                                    }
                                                        
                                                }
                                                    
                                                else
                                                    propertyInfo.SetValue(instance, "No Mapping", null);
                                            }
                                        }

                                        presource = resource;
                                        pinstance = instance;
                                        partIterator++;
                                    }
                                }
                                else
                                {
                                    var propertyInfo = type.GetProperty(view.Value.Name);

                                    if (propertyInfo != null)
                                    {
                                        var link = SearchAndFindPropertyLink(@base.View.Name, view.Value.Name, "cassandra");

                                        if (link != null)
                                        {
                                            if (!string.IsNullOrEmpty(view.Key.Alias))
                                            {
                                                if(row.GetValue<object>(view.Key.Alias) is not null)
                                                    propertyInfo.SetValue(model, row.GetValue<object>(view.Key.Alias).ToString(), null);
                                            }
                                                
                                            else
                                            {
                                                if (row.GetValue<object>(view.Key.Name) is not null)
                                                    propertyInfo.SetValue(model, row.GetValue<object>(view.Key.Name).ToString(), null);
                                            }
                                                
                                        }
                                            
                                        else
                                            propertyInfo.SetValue(model, "No Mapping", null);
                                    }
                                }
                            }

                            //add mode to result list
                            results.Add(model);
                        }
                    }
                }
            }

            return results;
        }

        public static List<ResultsModel> Create(Codex codex, List<IRecord> records)
        {
            var results = new List<ResultsModel>();
          
            var type = typeof(ResultsModel);

            if (codex.DataModel != null)
            {
                foreach (var expr in codex.DataModel.Value)
                {
                    if (expr is DataExpr)
                    {
                        //get primary model for selection base
                        var data = (DataExpr)expr;

                        var @base = Assistor.USchema.Single(x => x.View.Name == data.Value);

                        foreach (var row in records)
                        {
                            var model = new ResultsModel();

                            foreach (var view in codex.PropertyModel[0].Views)
                            {
                                if (view.Value.Name.IndexOf('.') > -1)
                                {
                                    var parts = view.Value.Name.Split('.');

                                    Type? @class = null;

                                    object? instance = null;
                                    object? pinstance = null;
                                    Resources resource = null;
                                    Resources presource = null;
                                    var partIterator = 0;

                                    foreach (var part in parts)
                                    {
                                        if (partIterator == 0)
                                            resource = @base.View.Resources.Single(x => x.Property == part);
                                        else
                                        {
                                            var schema = Assistor.USchema.Single(x => x.View.Name == presource?.Property);

                                            resource = schema.View.Resources.Single(x => x.Property == part);
                                        }

                                        if (resource?.Metadata == "class")
                                        {
                                            @class = GetConcreteClassType(resource.Type);

                                            if (partIterator > 0)
                                                instance = pinstance.GetType().GetProperty(resource.Property).GetValue(pinstance, null);
                                            else
                                                instance = model.GetType().GetProperty(resource.Property).GetValue(model, null);
                                        }
                                        else
                                        {
                                            var propertyInfo = @class?.GetProperty(resource.Property);

                                            if (propertyInfo != null)
                                            {

                                                var link = SearchAndFindPropertyLink(presource.Property, resource.Property, "neo4j");

                                                if (link != null)
                                                {
                                                    if (!string.IsNullOrEmpty(view.Key.Alias))
                                                    {
                                                        if(row[view.Key.Alias + "." + view.Key.Name] is Dictionary<string, object>)
                                                        {
                                                            var properties = (Dictionary<string, object>)row[view.Key.Alias + "." + view.Key.Name];

                                                            var json = link.Property.Split(".");

                                                            propertyInfo.SetValue(instance, properties[json[json.Length - 1]].ToString(), null);
                                                            
                                                        }
                                                        else
                                                        {
                                                            propertyInfo.SetValue(instance, row[view.Key.Alias + "." + view.Key.Name].ToString(), null);
                                                        }
                                                    }
                                                        
                                                    else
                                                        propertyInfo.SetValue(instance, row[view.Key.Name].ToString(), null);
                                                }
                                                else
                                                    propertyInfo.SetValue(model, "No Mapping", null);
                                            }
                                        }

                                        presource = resource;
                                        pinstance = instance;
                                        partIterator++;
                                    }
                                }
                                else
                                {
                                    var propertyInfo = type.GetProperty(view.Value.Name);

                                    if (propertyInfo != null)
                                    {
                                        var link = SearchAndFindPropertyLink(@base.View.Name, view.Value.Name, "neo4j");

                                        if (link != null)
                                        {
                                            if(!string.IsNullOrEmpty(view.Key.Alias))
                                                propertyInfo.SetValue(model, row[view.Key.Alias + "." + view.Key.Name].ToString(), null);
                                            else
                                                propertyInfo.SetValue(model, row[view.Key.Name].ToString(), null);
                                        }
                                        else
                                            propertyInfo.SetValue(model, "No Mapping", null);
                                    }
                                }
                            }

                            //add mode to result list
                            results.Add(model);
                        }
                    }
                }
            }

            return results;
        }

        public static List<ResultsModel> Create(Codex codex, List<object> records)
        {
            var results = new List<ResultsModel>();

            var type = typeof(ResultsModel);

            if (codex.DataModel != null)
            {
                foreach (var expr in codex.DataModel.Value)
                {
                    if (expr is DataExpr)
                    {
                        //get primary model for selection base
                        var data = (DataExpr)expr;

                        var @base = Assistor.USchema.Single(x => x.View.Name == data.Value);

                        foreach (var row in records)
                        {
                            var model = new ResultsModel();

                            foreach (var view in codex.PropertyModel[0].Views)
                            {
                                if (view.Value.Name.IndexOf('.') > -1)
                                {
                                    var parts = view.Value.Name.Split('.');

                                    Type? @class = null;
                                    
                                    object? instance = null;
                                    object? pinstance = null;
                                    Resources resource = null;
                                    Resources presource = null;
                                    var partIterator = 0;

                                    foreach (var part in parts)
                                    {
                                        if (partIterator == 0)
                                            resource = @base.View.Resources.Single(x => x.Property == part);
                                        else
                                        {
                                            var schema = Assistor.USchema.Single(x => x.View.Name == presource?.Property);

                                            resource = schema.View.Resources.Single(x => x.Property == part);
                                        }

                                        if (resource?.Metadata == "class")
                                        {
                                            @class = GetConcreteClassType(resource.Type);
                                            
                                            if(partIterator > 0)
                                                instance = pinstance.GetType().GetProperty(resource.Property).GetValue(pinstance, null);
                                            else
                                                instance = model.GetType().GetProperty(resource.Property).GetValue(model, null);
                                        }
                                        else
                                        {
                                            var propertyInfo = @class?.GetProperty(resource.Property);

                                            if (propertyInfo != null)
                                            {
                                                var token = JToken.Parse(row.ToString());

                                                if (token != null)
                                                {
                                                    var link = SearchAndFindPropertyLink(presource.Property, resource.Property, "redis");

                                                    if (link != null && token[link.Property] != null)
                                                        propertyInfo.SetValue(instance, token[link.Property].ToString(), null);
                                                    else
                                                        propertyInfo.SetValue(instance, "No Mapping", null);
                                                }
                                                else
                                                    propertyInfo.SetValue(model, "No Mapping", null);
                                            }
                                        }

                                        presource = resource;
                                        pinstance = instance;
                                        partIterator++;
                                    }
                                }
                                else
                                {
                                    var propertyInfo = type.GetProperty(view.Value.Name);

                                    if (propertyInfo != null)
                                    {
                                        var token = JToken.Parse(row.ToString());

                                        if (token != null)
                                        {
                                            var link = SearchAndFindPropertyLink(@base.View.Name, view.Value.Name, "redis");

                                            if (link != null && token[link.Property] != null)
                                                //if (token[view.Key.Name] != null)
                                                propertyInfo.SetValue(model, token[view.Key.Name].ToString(), null);
                                            else
                                                propertyInfo.SetValue(model, "No Mapping", null);

                                        }
                                        else
                                            propertyInfo.SetValue(model, "No Mapping", null);
                                    }
                                }
                            }

                            //add mode to result list
                            results.Add(model);
                        }
                    }
                }
            }

            return results;
        }

        public static List<ResultsModel> Create(Codex codex, IAsyncCursor<BsonDocument> records)
        {
            var results = new List<ResultsModel>();

            var type = typeof(ResultsModel);

            if (codex.DataModel != null)
            {
                foreach (var expr in codex.DataModel.Value)
                {
                    if (expr is DataExpr)
                    {
                        //get primary model for selection base
                        var data = (DataExpr)expr;

                        var @base = Assistor.USchema.Single(x => x.View.Name == data.Value);

                        foreach (var row in records.ToList())
                        {
                            var model = new ResultsModel();

                            foreach (var view in codex.PropertyModel[0].Views)
                            {
                                if (view.Value.Name.IndexOf('.') > -1)
                                {
                                    var parts = view.Value.Name.Split('.');

                                    Type? @class = null;

                                    object? instance = null;
                                    object? pinstance = null;
                                    Resources resource = null;
                                    Resources presource = null;
                                    var partIterator = 0;

                                    foreach (var part in parts)
                                    {
                                        if (partIterator == 0)
                                            resource = @base.View.Resources.Single(x => x.Property == part);
                                        else
                                        {
                                            var schema = Assistor.USchema.Single(x => x.View.Name == presource?.Property);

                                            resource = schema.View.Resources.Single(x => x.Property == part);
                                        }

                                        if (resource?.Metadata == "class")
                                        {
                                            @class = GetConcreteClassType(resource.Type);

                                            if (partIterator > 0)
                                                instance = pinstance.GetType().GetProperty(resource.Property).GetValue(pinstance, null);
                                            else
                                                instance = model.GetType().GetProperty(resource.Property).GetValue(model, null);
                                        }
                                        else
                                        {
                                            var propertyInfo = @class?.GetProperty(resource.Property);

                                            if (propertyInfo != null)
                                            {
                                                var link = SearchAndFindPropertyLink(presource.Property, resource.Property, "mongodb");

                                                if (link != null)
                                                {
                                                    if (row.TryGetElement(view.Key.Alias, out var ekement))
                                                        propertyInfo.SetValue(instance, row.GetElement(view.Key.Alias).Value.ToString(), null);
                                                    else
                                                        propertyInfo.SetValue(instance, "No Mapping", null);
                                                }
                                                else
                                                    propertyInfo.SetValue(instance, "No Mapping", null);

                                                //var token = JToken.Parse(row.ToString());

                                                //if (token != null)
                                                //{
                                                //    var link = SearchAndFindPropertyLink(presource.Property, resource.Property, "redis");

                                                //    if (link != null && token[link.Property] != null)
                                                //        propertyInfo.SetValue(instance, token[link.Property].ToString(), null);
                                                //    else
                                                //        propertyInfo.SetValue(instance, "No Mapping", null);
                                                //}
                                                //else
                                                //    propertyInfo.SetValue(model, "No Mapping", null);
                                            }
                                        }

                                        presource = resource;
                                        pinstance = instance;
                                        partIterator++;
                                    }
                                }
                                else
                                {
                                    var propertyInfo = type.GetProperty(view.Value.Name);

                                    if (propertyInfo != null)
                                    {
                                        var link = SearchAndFindPropertyLink(@base.View.Name, view.Value.Name, "mongodb");

                                        if(link != null)
                                        {
                                            if (row.TryGetElement(view.Key.Alias, out var ekement))
                                                propertyInfo.SetValue(model, row.GetElement(view.Key.Alias).Value.ToString(), null);
                                            else
                                                propertyInfo.SetValue(model, "No Mapping", null);
                                        }
                                        else
                                            propertyInfo.SetValue(model, "No Mapping", null);
                                    }
                                }
                            }

                            //add mode to result list
                            results.Add(model);
                        }
                    }
                }
            }

            return results;
        }

        private static Link? SearchAndFindPropertyLink(string model, string property, string database)
        {
            return Assistor.USchema
                        .Where(x => x.View.Name == model)
                        .SelectMany(x => x.View.Resources)
                        .Where(x => x.Property == property)
                        .SelectMany(x => x.Link)
                        .Where(x => x.Target == database)
                        .FirstOrDefault();
        }

        private static Type? GetConcreteClassType(string fullpath, string targetProperty)
        {
            var parts = fullpath.Split(".");

            var @base = Assistor.USchema.SelectMany(x => x.View.Resources.Where(x => x.Property == parts[0])).First();

            if(@base.Property == targetProperty)
                return GetConcreteClassType(@base.Type);
            else
            {
                foreach (var part in parts)
                {
                    if (@base.Property != targetProperty && part == targetProperty)
                    {
                        var schema = Assistor.USchema.First(x => x.View.Name == @base.Property);

                        var resource = schema.View.Resources.SingleOrDefault(x => x.Property == targetProperty);

                        if(resource != null)
                            return GetConcreteClassType(resource.Type);
                    }
                }
            }
            
            return default;
        }

        private static Type? GetConcreteClassType(string type)
        {
            if (type == "student")
                return typeof(ResultsModel);
            else if (type == "faculty")
                return typeof(FacultyModel);
            else if (type == "course")
                return typeof(CourseModel);
            else if (type == "subject")
                return typeof(SubjectModel);
            else if (type == "address")
                return typeof(AddressModel);
            else if (type == "country")
                return typeof(CountryModel);
            else if (type == "contact")
                return typeof(ContactModel);
            else if (type == "register")
                return typeof(RegisterModel);
            else if (type == "transcript")
                return typeof(TranscriptModel);

            return default;
        }
    }
}
