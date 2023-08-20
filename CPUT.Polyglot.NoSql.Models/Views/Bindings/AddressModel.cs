using System.Diagnostics.Metrics;

namespace CPUT.Polyglot.NoSql.Models.Views.Bindings
{
    public class AddressModel
    {
        public string streetno { get; set; }
        public string street { get; set; }
        public string postaladdress { get; set; }
        public string postalcode { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public CountryModel country { get; set; }

        public AddressModel()
        {
            country = new CountryModel();
        }
    }
}
