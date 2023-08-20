using CPUT.Polyglot.NoSql.Models.Views.Bindings;

namespace CPUT.Polyglot.NoSql.Models.Views.Bindings
{
    public class TranscriptModel
    {
        public string course { get; set; }
        public string subject { get; set; }
        public string result { get; set; }
        public string symbol { get; set; }

        public TranscriptModel()
        {
        }
    }
}
