using CPUT.Polyglot.NoSql.Interface.Logic;
using NUnit.Framework;
using System;

namespace CPUT.Polyglot.NoSql.Tests.Integration
{
    public class QueryControllerTests
    {
        private readonly IServiceLogic _serviceLogic;

        public QueryControllerTests(IServiceLogic serviceLogic)
        {
            _serviceLogic = serviceLogic;
        }

        [Test]
        public void GetStudentWithFilter()
        {
            string input = string.Empty;

            try
            {
                input = @"";

                _serviceLogic.Query(input);

                Assert.Pass();
            }
            catch(Exception ex)
            {
                Assert.Fail();
            }
            
        }
    }
}
