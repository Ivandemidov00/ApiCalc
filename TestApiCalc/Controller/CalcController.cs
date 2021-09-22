using System;
using Microsoft.AspNetCore.Mvc;
using TestApiCalc.Model;
using TestApiCalc.Service;

namespace TestApiCalc.Controller
{
    [Route("/api/")]
    public class CalcController:ControllerBase
    {
        private IService _Service;

        public CalcController(IService service)
        {
            _Service = service;
        }

        [HttpPost("calc")]
        public IActionResult Authenticate(Expression model)
        {
            Int32 result = _Service.GetResult(model);
            return Ok(result);
        }
    }
}