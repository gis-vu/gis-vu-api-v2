﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using SearchGIS;

namespace GIS.VU.API.Controllers
{
    
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly SearchEngine _searchEngine;

        public RouteController(SearchEngine searchEngine)
        {
            _searchEngine = searchEngine;
        }

        [Route("")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }


        [Route("api/[controller]")]
        [HttpPost]
        public ActionResult<RouteSearchResponseDTO> Post([FromBody] RouteSearchRequestDTO request)
        {
            return _searchEngine.FindRoute(request);
        }
    }
}
