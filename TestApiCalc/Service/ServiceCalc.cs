using System;
using TestApiCalc.Model;

namespace TestApiCalc.Service
{
    public class ServiceCalc: IService
    {
        private IOperation _operation;
        ServiceCalc() {
            
        }

        private Int32 Command(IOperation operation)
        {
            return 
        }

        public Int32 GetResult(Expression expression)
        {
           Int32 result =  expression.Operation switch
            {
                Operators.plus =>  Add(expression.First,expression.Second),
                Operators.mult =>  Mul(expression.First,expression.Second),
               // _ => throw new ArgumentException("Недопустимая операция")
                _ => throw new ArgumentOutOfRangeException()
           };
            return result;
        }

        public Int32 Add(Int32 FirstArg, Int32 SecondArg)
        {
            return FirstArg + SecondArg;
        }
        public Int32 Mul(Int32 FirstArg, Int32 SecondArg)
        {
            return FirstArg * SecondArg;
        }
        
    }
}