using System;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Await : IExpression
    {
        private readonly IExpression _operand;

        internal Await(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine).Value;

            if (value.Is(out Task? task))
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