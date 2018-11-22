using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GIS.VU.API.Controllers
{
    
    [ApiController]
    public class RouteController : ControllerBase
    {
        private RouteSearchEngine _routeSearchEngine;

        public RouteController(RouteSearchEngine routeSearchEngine)
        {
            _routeSearchEngine = routeSearchEngine;
        }

        [Route("")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }


        [Route("api/[controller]")]
        [HttpPost]
        public ActionResult<RouteSearchResponse> Post([FromBody] RouteSearchRequest request)
        {
            return _routeSearchEngine.FindRoute(request);
        }
    }
}
