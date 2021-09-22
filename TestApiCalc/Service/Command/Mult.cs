using System;
using TestApiCalc.Model;

namespace TestApiCalc.Service.Command
{
    public class Mult:IOperation
    {
        public override Int32 getResultExpression(Expression model)
        {
            return model.First * model.Second;
        }
    }
}