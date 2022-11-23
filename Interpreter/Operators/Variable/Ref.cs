using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Ref : IExpression
    {
        private readonly IExpression _operand;

        internal Ref(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not Pointer pointer)
                throw new Throw("The right part of a reference must be a variable");

            if (pointer is UndefinedPointer undefined)
                throw new Throw($"Variable {undefined.Name} was not defined in scope");

            return new Reference(pointer);
        }
    }
}