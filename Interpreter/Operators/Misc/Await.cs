using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Await : IExpression
    {
        private readonly IExpression _operand;

        internal Await(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is Task task)
                return task.Await();

            throw new Throw($"Cannot apply operator 'await' on type {value.GetType().ToString().ToLower()}");
        }
    }
}