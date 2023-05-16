using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Mapper;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Views;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex;
using CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple;
using System.Collections.Generic;
using System.Linq;

namespace CPUT.Polyglot.NoSql.Logic.Core
{
    public class Validator : IValidator
    {
        private ISchema _schema;
        private List<USchema> _global;

        public Validator(ISchema schema)
        {
            _schema = schema;

            //load unified schema
            _global = _schema.UnifiedView();
        }

        public Validators GlobalSchema(BaseExpr baseExpr)
        {
            DataModelExpr dataModelExpr = null;

            //data models
            HashSet<string> uniqueModels = new HashSet<string>();
            HashSet<string> uniqueMatchModels = new HashSet<string>();
            List<string> unmatchModels = new List<string>();
            List<string> matchmodels = new List<string>();
            List<string> specifiedmodels = new List<string>();

            //properties
            List<string> inputProperties = new List<string>();
            List<string> matchedProperties = new List<string>();
            HashSet<string> uniqueFields = new HashSet<string>();
            HashSet<string> uniqueMatchFields = new HashSet<string>();
            List<string> unmatchFields = new List<string>();

            bool valid = true;
            string message = "Query passed unified schema.";

            dataModelExpr = (DataModelExpr)baseExpr.ParseTree.Single(x => x.GetType().Equals(typeof(DataModelExpr)));

            //verify data models
            foreach (DataExpr expr in dataModelExpr.Value)
            {
                foreach (var model in _global.Where(x => x.View.Name == expr.Value).Select(x => x.View))
                    matchmodels.Add(model.Name);

                specifiedmodels.Add(expr.Value);
            }

            //get unique specified fields
            uniqueModels = specifiedmodels.ToHashSet();
            uniqueMatchModels = matchmodels.ToHashSet();

            //extract unmatches fields against global schem fields
            uniqueModels.SymmetricExceptWith(uniqueMatchModels);
            //convert to a list
            unmatchModels = uniqueModels.ToList();

            if (unmatchModels?.Count() > 0)
            {
                valid = false;
                message = string.Format("Invalid models specified: {0}", string.Join(",", unmatchModels));
            }
            else
            {
               
                //verify fields
                foreach (var expr in baseExpr.ParseTree)
                {
                    if (expr is DeclareExpr)
                    {
                        inputProperties.AddRange(GetProperties(((DeclareExpr)expr).Value));
                    }
                    else if (expr is PropertiesExpr)
                    {
                        inputProperties.AddRange(GetProperties(((PropertiesExpr)expr).Value));
                    }
                    else if (expr is LinkExpr)
                    {
                        inputProperties.AddRange(GetProperties(((LinkExpr)expr).Value));
                    }
                    else if (expr is FilterExpr)
                    {
                        inputProperties.AddRange(GetProperties(((FilterExpr)expr).Value));
                    }
                    else if (expr is GroupByExpr)
                    {
                        inputProperties.AddRange(GetProperties(((GroupByExpr)expr).Value));
                    }

                    if (expr is DeclareExpr || expr is PropertiesExpr || expr is LinkExpr || expr is FilterExpr || expr is GroupByExpr)
                        matchedProperties.AddRange(GetDataModels(dataModelExpr, inputProperties));
                }

                //get unique specified fields
                uniqueFields = inputProperties.ToHashSet();

                if (matchedProperties != null)
                {
                    uniqueMatchFields = matchedProperties.ToHashSet();

                    //extract unmatches fields against global schem fields
                    uniqueFields.SymmetricExceptWith(uniqueMatchFields);

                    //convert to a list
                    unmatchFields = uniqueFields.ToList();
                }

                if (unmatchFields?.Count() > 0)
                {
                    valid = false;
                    message = string.Format("Invalid fields specified: {0}", string.Join(",", unmatchFields));
                }
            }

            return new Validators
            {
                Success = valid,
                Message = message
            };
        }


        #region private methods

        private List<string> GetDataModels(DataModelExpr dataModelExpr, List<string> fields)
        {
            List<string> matchedFields = new List<string>();

            foreach (DataExpr data in dataModelExpr.Value)
            {
                foreach (var properties in _global.Where(x => x.View.Name == data.Value).SelectMany(x => x.View.Resources))
                {
                    foreach (var column in fields)
                    {
                        if (column == properties.Property)
                            matchedFields.Add(column);
                    }
                }
            }

            return matchedFields;
        }

        private List<string> GetProperties(BaseExpr[] baseExpr)
        {
            List<string> specifiedFields = new List<string>();

            foreach (var part in baseExpr)
            {
                if (part is PropertyExpr)
                {
                    PropertyExpr property = (PropertyExpr)part;

                    if (!specifiedFields.Contains(property.Value))
                        specifiedFields.Add(property.Value);
                }
                else if (part is FunctionExpr)
                {
                    FunctionExpr function = (FunctionExpr)part;

                    foreach (var func in function.Value)
                    {
                        PropertyExpr property = (PropertyExpr)func;

                        if (!specifiedFields.Contains(property.Value))
                            specifiedFields.Add(property.Value);
                    }
                }
                else if (part is GroupExpr)
                {
                    GroupExpr group = (GroupExpr)part;

                    OperatorExpr ops = (OperatorExpr)group.Value;

                    TermExpr leftTerm = (TermExpr)ops.Left;

                    if (!specifiedFields.Contains(leftTerm.Value))
                        specifiedFields.Add(leftTerm.Value);


                    if (ops.Right is TermExpr)
                    {
                        TermExpr rightTerm = (TermExpr)ops.Right;

                        if (!specifiedFields.Contains(rightTerm.Value))
                            specifiedFields.Add(rightTerm.Value);
                    }
                }
            }

            return specifiedFields;
        }

        #endregion
    }
}
