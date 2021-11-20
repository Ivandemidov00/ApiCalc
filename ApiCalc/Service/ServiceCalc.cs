using ApiCalc.Service.Interface;

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
                Operators.PlMu => PlMu(expression),
                _ => throw new ArgumentException("Недопустимая операция")
            };
            return result;
        }
        
        
        private static Opertions Sum = (expression) => expression.First + expression.Second;
        
        private static Opertions Exp = (expression) => Convert.ToInt32(Math.Pow(expression.First, expression.Second));

        private static Opertions Mul = (expression) => expression.First * expression.Second;
        
        private Opertions PlMu = (expression) => Mul(expression) * expression.Second;


    }
}