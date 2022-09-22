using CPUT.Polyglot.NoSql.Interface.Logic;
using CPUT.Polyglot.NoSql.Interface.Middleware;
using System;

namespace CPUT.Polyglot.NoSql.Logic
{
    public class ServiceLogic : IServiceLogic
    {
        private readonly IPolyCommand _unifiedCommand;
        private readonly IDataFactory _dataFactory;

        public ServiceLogic(IPolyCommand unifiedCommand, IDataFactory dataFactory)
        {
            _unifiedCommand = unifiedCommand;
            _dataFactory = dataFactory;
        }

        public string LoadMockData()
        {
            try
            {
                //data test data
                _dataFactory.LoadGraph();

                _dataFactory.LoadKeyValue();

                _dataFactory.LoadColumnar();

                _dataFactory.LoadDocument();
            }
            catch(Exception ex)
            {
                return "Error";
            }

            return "Data Loafd Completed";
        }

        //private Printer _printer = new Printer();

        public string Query(string statement)
        {
            

            return "OK";
        }

      
    }
}
