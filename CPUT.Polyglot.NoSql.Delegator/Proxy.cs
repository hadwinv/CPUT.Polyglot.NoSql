using CPUT.Polyglot.NoSql.Interface.Delegator;
using CPUT.Polyglot.NoSql.Interface.Repos;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models._data;
using Newtonsoft.Json;
using CPUT.Polyglot.NoSql.Models.Translator;

namespace CPUT.Polyglot.NoSql.Delegator
{
    public class Proxy : IProxy
    {
        private IRedisRepo _redisjRepo;
        private ICassandraRepo _cassandraRepo;
        private IMongoDbRepo _mongoRepo;
        private INeo4jRepo _neo4jRepo;

        public Proxy(IRedisRepo redisjRepo, ICassandraRepo cassandraRepo, IMongoDbRepo mongoRepo, INeo4jRepo neo4jRepo)
        {
            _redisjRepo = redisjRepo;
            _cassandraRepo = cassandraRepo;
            _mongoRepo = mongoRepo;
            _neo4jRepo = neo4jRepo;
        }

        public Models.Result Forward(Constructs construct)
        {
            Models.Result result = null;

            if (construct.Target == Common.Helpers.Utils.Database.REDIS)
            {
                result = _redisjRepo.Execute(construct);
            }

            return result;
        }

        public void Load()
        {
            try
            {
                //load mock data
                var dataset = MockFullDataset();

                //data test data
                //_redisjRepo.Load(dataset);

                _cassandraRepo.Load(dataset);

                //_mongoRepo.Load(dataset);

                //_neo4jRepo.Load(dataset);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
        }

        #region Data

        private List<UDataset> MockFullDataset()
        {
            List<UDataset> dataset = new List<UDataset>();
            UFaculty uFaculty = null;
            UCourse uCourse = null;
            USubject uSubject = null;

            UStudent uStudent = null;
            UProfile uProfile = null;
            UAddress uAddress = null;
            ULocation uLocation = null;
            UMarks uMarks = null;

            //get course
            var courses = GetCourses();

            //get pupils
            var persons = GetPersons();

            UDataset data = new UDataset();

            #region Load Course Info

            foreach (var course in courses.OrderBy(x => x.faculty))
            {
                uFaculty = data.Faculties.SingleOrDefault(x => x.Description == course.faculty);

                if (uFaculty == null)
                {
                    uFaculty = new UFaculty
                    {
                        Id = GenerateId(),
                        Code = GenerateThreeRandomLetters(),
                        Description = course.faculty
                    };

                    //add faculty
                    data.Faculties.Add(uFaculty);
                }

                uCourse = new UCourse
                {
                    Id = GenerateId(),
                    Code = course.code,
                    Description = course.name
                };

                //add courses
                uFaculty.Courses.Add(uCourse);

                foreach (var subject in course.subject)
                {
                    uSubject = new USubject
                    {
                        Id = GenerateId(),
                        Code = subject.short_code,
                        Description = subject.name,
                        Price = decimal.Parse(subject.cost),
                        Duration = 6
                    };

                    //add subjects
                    uCourse.Subjects.Add(uSubject);
                }
            }

            #endregion

            #region Load Student Info

            var courseIds = data.Faculties.SelectMany(x => x.Courses.Select(y => y.Id)).ToList();
            var trackCourses = new List<string>();
            var studentCount = 0;

            foreach (var person in persons)
            {
                uStudent = new UStudent
                {
                    Id = person.ID,
                    IdNumber = person.IDNumber.Replace("-", "0"),
                    Title = person.Gender == "F" ? "Mrs" : "Mr",
                    Name = person.FirstName,
                    Surname = person.LastName,
                    DOB = person.DOB,
                    Gender = person.Gender,
                    Email = person.Email,
                    MobileNo = person.PhoneNo,
                    Language = person.Language
                };

                int index = new Random().Next(courseIds.Count);
                int assignRandomCourse = courseIds[index];
                var randomCourse = data.Faculties.SelectMany(x => x.Courses).SingleOrDefault(y => y.Id == assignRandomCourse);

                //generate data
                var date = DateTime.Now.AddMonths(-GenerateIdRange(0, 48));

                uProfile = new UProfile
                {
                    StudentNo = GenerateStudentId(),
                    Username = (person.LastName.Substring(0, 1) + person.FirstName.Replace(" ", "")).ToLower(),
                    Password = GeneratePassword(),
                    IPAddress = person.IPAddress,
                    RegistrationDate = date,
                    GraduatedDate = (date > DateTime.Now ? null : date.AddYears(1)),
                    Course = randomCourse
                };

                var country = GenerateCountry();

                uLocation = new ULocation
                {
                    Id = GenerateId(),
                    Province = GetProvince(country),
                    CountryCode = GetCountryCode(country),
                    Country = country
                };

                uAddress = new UAddress
                {
                    Id = GenerateId(),
                    StreetNo = GenerateIdRange(0, 150),
                    Street = person.Street,
                    StreetAddress = person.StreetAddress,
                    PostalAddress = person.PostalAddress,
                    PostalCode = GenerateIdRange(1000, 4000).ToString(),
                    City = person.City,
                    Location = uLocation
                };

                if (uProfile.Course != null)
                {
                    foreach (var subject in uProfile.Course.Subjects)
                    {
                        var marks = GenerateMarks();

                        uMarks = new UMarks
                        {
                            Id = GenerateId(),
                            CourseCode = uProfile.Course.Code,
                            Course = uProfile.Course.Description,
                            SubjectCode = subject.Code,
                            Subject = subject.Description,
                            Score = marks,
                            Grade = GetGrades(marks)
                        };

                        uStudent.Marks.Add(uMarks);
                    }
                }

                uStudent.Profile.Course = uCourse;
                uStudent.Profile = uProfile;
                uStudent.Address = uAddress;

                data.Students.Add(uStudent);

                studentCount++;
            }

            #endregion

            //add complete dataset
            dataset.Add(data);

            return dataset;
        }

        private List<MockCourse> GetCourses()
        {
            List<MockCourse> response = null;
            //
            //Open the json file using a stream reader.
            using (var sr = new StreamReader(@"Repos/_data/00_template_faculty_course_subject.json"))
            {
                // Read the stream as a string, and write the string to the console.
                var data = sr.ReadToEnd();

                if (!string.IsNullOrEmpty(data))
                {
                    response = JsonConvert.DeserializeObject<List<MockCourse>>(data);
                }
            }

            return response;
        }

        private List<MockPerson> GetPersons()
        {
            List<MockPerson> response = null;

            //Open the json file using a stream reader.
            using (var sr = new StreamReader(@"Repos/_data/01_template_student_add_loc.json"))
            {
                // Read the stream as a string, and write the string to the console.
                var data = sr.ReadToEnd();

                if (!string.IsNullOrEmpty(data))
                {
                    response = JsonConvert.DeserializeObject<List<MockPerson>>(data);
                }
            }

            return response;
        }

        #endregion

        #region Helpers - Data generaors

        private string GenerateThreeRandomLetters()
        {
            Random rnd = new Random();
            var letters = string.Empty;
            char letter;
            int asciiIndex = 0;

            for (var i = 0; i < 2; i++)
            {
                asciiIndex = rnd.Next(65, 91); //ASCII character codes 65-90

                letter = Convert.ToChar(asciiIndex); //produces any char A-Z

                letters += letter;
            }

            return letters;
        }

        private int GenerateId()
        {
            return new Random().Next(10000, 9999999);
        }

        private int GenerateStudentId()
        {
            return new Random().Next(100000000, 999999990);
        }

        private string GeneratePassword()
        {
            int passwordSize = 10;
            char[] _password = new char[passwordSize];
            string charSet = ""; // Initialise to blank
            System.Random _random = new Random();
            int counter;

            // Build up the character set to choose from
            charSet += "abcdefghijklmnopqursuvwxyz";

            charSet += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            charSet += "123456789";

            charSet += @"!@£$%^&*()#€";

            for (counter = 0; counter < passwordSize; counter++)
            {
                _password[counter] = charSet[_random.Next(charSet.Length - 1)];
            }

            return String.Join(null, _password);
        }


        private int GenerateIdRange(int start, int end)
        {
            return new Random().Next(start, end);
        }

        private double GenerateMarks()
        {
            return new Random().Next(0, 100);
        }

        private string GetProvince(string country)
        {
            string province = string.Empty;

            switch (country)
            {
                case "South Africa":
                    province = GenerateSAProvince();
                    break;
                case "Angola":
                    province = GenerateAngolaProvince();
                    break;
                case "Nigeria":
                    province = GenerateNigeriaProvince();
                    break;
                case "Namibia":
                    province = GenerateNamibiaProvince();
                    break;
                case "Botswana":
                    province = GenerateBotswanaProvince();
                    break;
                case "Egypt":
                    province = GenerateEgyptProvince();
                    break;
                default:
                    province = GenerateTunisiaProvince();
                    break;
            }

            return province;
        }

        private string GenerateSAProvince()
        {
            var list = new List<string> {
                "Eastern Cape",
                "Free State",
                "Gauteng",
                "KwaZulu-Natal",
                "Limpopo",
                "Mpumalanga",
                "Northern Cape",
                "North West",
                "Western Cape"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateAngolaProvince()
        {
            var list = new List<string> {
                "Bengo",
                "Benguela",
                "Bié",
                "Cabinda",
                "Cuando Cubango",
                "Cuanza Norte",
                "Cuanza Sul",
                "Cunene",
                "Huambo",
                "Huíla",
                "Luanda",
                "Lunda Norte",
                "Lunda Sul",
                "Malanje",
                "Moxico",
                "Namibe",
                "Uíge",
                "Zaire"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateNigeriaProvince()
        {
            var list = new List<string> {
                "Bauchi",
                "Bida",
                "Bornu",
                "Kabba",
                "Kontagora",
                "Lower Benue or Nassarawa",
                "Illorin",
                "Muri",
                "Sokoto",
                "Upper Bema",
                "Zaria"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateNamibiaProvince()
        {
            var list = new List<string> {
                "Caprivi",
                "Erongo",
                "Hardap",
                "Karas",
                "Kavango West",
                "Kavango East",
                "Khomas",
                "Kunene",
                "Ohangwena",
                "Omaheke",
                "Omusati",
                "Oshana",
                "Oshikoto",
                "Otjozondjupa"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateBotswanaProvince()
        {
            var list = new List<string> {
                "Central",
                "Ghanzi",
                "Kgalagadi",
                "Kgatleng",
                "Kweneng",
                "North East",
                "North West",
                "South East",
                "Southern"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateEgyptProvince()
        {
            var list = new List<string> {
               "Alexandria Governorate",
                "Aswan Governorate",
                "Asyut Governorate",
                "Beheira Governorate",
                "Beni Suef Governorate",
                "Cairo Governorate",
                "Dakahlia Governorate",
                "Damietta Governorate",
                "Faiyum Governorate",
                "Gharbia Governorate",
                "Giza Governorate",
                "Ismailia Governorate",
                "Kafr El Sheikh Governorate",
                "Luxor Governorate",
                "Matruh Governorate",
                "Minya Governorate",
                "Monufia Governorate",
                "New Valley Governorate",
                "North Sinai Governorate",
                "Port Said Governorate[5]",
                "Qalyubia Governorate",
                "Qena Governorate",
                "Red Sea Governorate",
                "Sharqia Governorate",
                "Sohag Governorate",
                "South Sinai Governorate",
                "Suez Governorate"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateTunisiaProvince()
        {
            var list = new List<string> {
                "Ariana",
                "Béja",
                "Ben Arous",
                "Bizerte",
                "Gabès",
                "Gafsa",
                "Jendouba",
                "Kairouan",
                "Kasserine",
                "Kebili",
                "Kef",
                "Mahdia",
                "Manouba",
                "Medenine",
                "Monastir",
                "Nabeul",
                "Sfax",
                "Sidi Bouzid",
                "Siliana",
                "Sousse",
                "Tataouine",
                "Tozeur",
                "Tunis",
                "Zaghouan"};

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GenerateCountry()
        {
            var list = new List<string> { "South Africa", "Angola", "Nigeria", "Namibia", "Botswana", "Egypt", "Tunisia" };

            var index = new Random().Next(list.Count);

            return list[index];
        }

        private string GetCountryCode(string country)
        {
            string code = string.Empty;

            switch (country)
            {
                case "South Africa":
                    code = "ZA";
                    break;
                case "Angola":
                    code = "AO";
                    break;
                case "Nigeria":
                    code = "NG";
                    break;
                case "Namibia":
                    code = "NA";
                    break;
                case "Botswana":
                    code = "BW";
                    break;
                case "Egypt":
                    code = "EG";
                    break;
                default:
                    code = "TN";
                    break;
            }

            return code;
        }

        private string GetGrades(double mark)
        {
            if (mark >= 80)
            {
                return "A";
            }
            else if (mark >= 70)
            {
                return "B";
            }
            else if (mark >= 60)
            {
                return "C";
            }
            else if (mark >= 50)
            {
                return "D";
            }
            else if (mark >= 40)
            {
                return "E";
            }
            else if (mark >= 30)
            {
                return "F";
            }
            else
            {
                return "G";
            }
        }

        #endregion
    }
}