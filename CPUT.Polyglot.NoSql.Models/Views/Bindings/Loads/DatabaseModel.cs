using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Models.Views.Bindings.Loads
{
    public class DatabaseModel
    {
        public List<FacultyModel> Faculties { get; set; }

        public Dictionary<string, CourseModel> Courses { get; set; }

        public Dictionary<string, List<SubjectModel>> Subjects { get; set; }

        public List<ResultsModel> Students { get; set; }
    }
}
