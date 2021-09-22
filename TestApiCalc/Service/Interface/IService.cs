using System;
using TestApiCalc.Model;

namespace TestApiCalc.Service
{
    public interface IService
    {
        public  Int32 GetResult(Expression result);
      
    }
}