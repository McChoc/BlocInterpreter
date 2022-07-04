using System;
using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Misc
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

            throw new Throw($"Cannot apply operator 'await' on type {value.Type.ToString().ToLower()}");
        }
    }
}