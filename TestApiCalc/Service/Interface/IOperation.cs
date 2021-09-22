using System;
using TestApiCalc.Model;

namespace TestApiCalc.Service
{
    public abstract class IOperation
    {
        public abstract Int32 getResultExpression(Expression model);

        public Int32 getr()
        {
            return getResultExpression(model);
        }
    }
}