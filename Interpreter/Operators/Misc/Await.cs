using System;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Misc
{
    internal class Await : IExpression
    {
        private readonly IExpression _operand;

        internal Await(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value.Value.Is(out Task? task))
            {
                try
                {
                    return task!.Value.Result;
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            }

            throw new Throw($"Cannot apply operator 'await' on type {value.GetType().ToString().ToLower()}");
        }
    }
}