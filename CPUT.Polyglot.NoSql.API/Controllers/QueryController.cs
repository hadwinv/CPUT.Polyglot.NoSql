using CPUT.Polyglot.NoSql.Interface.Logic;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CPUT.Polyglot.NoSql.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : Controller
    {
        private readonly IServiceLogic _serviceLogic;

        public QueryController(IServiceLogic serviceLogic)
        {
            _serviceLogic = serviceLogic;
        }

        
        [HttpPost]
        public ActionResult<string> LoadData([FromBody] string request)
        {
            try
            {
                if (request != null)
                {
                    var response = _serviceLogic.LoadMockData();

                    return Ok(response);
                }
                return BadRequest("Error: Request is null");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost]
        public ActionResult<string> Execute([FromBody] string request)
        {
            try
            {
                if (request != null)
                {
                    var response = _serviceLogic.Query(request);

                    return Ok(response);
                }

                return BadRequest("Error: Request is null");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }
    }
}
