using System;
using ApiCalc.Model;
using ApiCalc.Service;
using Microsoft.AspNetCore.Mvc;

namespace ApiCalc.Controller
{
    [Route("/api/")]
    public class CalcController:ControllerBase
    {
        private IService _Service;
        public CalcController(IService service) => _Service = service;
        
        [HttpPost("calc")]
        public  IActionResult Calc(Expression model)
        {
            var result =  _Service.GetResult(model);
            return Ok(result);
        }
    }
}