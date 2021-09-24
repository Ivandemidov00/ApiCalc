using System;
using ApiCalc.Model;

namespace ApiCalc.Service
{
    public interface IService
    {
        public Int32 GetResult(Expression result);
      
    }
}