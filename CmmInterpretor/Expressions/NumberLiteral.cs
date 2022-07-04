using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    internal class NumberLiteral : IExpression
    {
        private readonly double _number;

        internal NumberLiteral(double number)
        {
            _number = number;
        }

        public IValue Evaluate(Call _) => new Number(_number);
    }
}