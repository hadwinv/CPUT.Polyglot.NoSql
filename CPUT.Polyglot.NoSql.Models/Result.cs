using CPUT.Polyglot.NoSql.Models.Views.Bindings;

namespace CPUT.Polyglot.NoSql.Models
{
    public class Result
    {
        public Common.Helpers.Utils.Database Source { get; set; }

        public List<ResultsModel> Data { get; set; }

        public bool Success  {get; set;}

        public string Executable { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

    }
}
