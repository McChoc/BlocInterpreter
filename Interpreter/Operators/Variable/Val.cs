using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Variable
{
    internal class Val : IExpression
    {
        private readonly IExpression _operand;

        internal Val(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            for (var i = 1; value.Value.Is(out Values.Reference? reference); i++)
            {
                if (i > call.Engine.HopLimit)
                    throw new Throw("The hop limit was reached");

                if (reference!.Variable is null)
                    throw new Throw("Invalid reference.");

                value = reference.Variable;
            }

            return value;
        }
    }
}