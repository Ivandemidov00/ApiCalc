using System;
using ApiCalc.Model;

namespace ApiCalc.Service
{
    public class ServiceCalc: IService
    {
        private delegate Int32 Opertions(Expression expression);
        public Int32 GetResult(Expression expression)
        {
           var result =  expression.Operation switch
            {
                Operators.Plus => Sum(expression),
                Operators.Mult => Mul(expression),
                Operators.Exp => Exp(expression),
                _ => throw new ArgumentException("Недопустимая операция")
            };
            return result;
        }
        
        
        private Opertions Sum = (expression) => expression.First + expression.Second;
        
        private Opertions Exp = (expression) => Convert.ToInt32(Math.Pow(expression.First, expression.Second));

        private Opertions Mul = (expression) => expression.First * expression.Second;

       

    }
}