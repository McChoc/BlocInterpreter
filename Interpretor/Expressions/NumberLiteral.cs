using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
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