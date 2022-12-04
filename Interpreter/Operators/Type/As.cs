using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record As : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal As(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (left is Void)
                throw new Throw($"Cannot apply operator 'as' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");

            if (right is not Type type)
                throw new Throw($"Cannot apply operator 'as' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");

            if (type.Value.Count != 1)
                throw new Throw("Cannot apply operator 'as' on a composite type");

            try
            {
                return type.Value.Single() switch
                {
                    ValueType.Void      => Void.Construct(new() { left }),
                    ValueType.Null      => Null.Construct(new() { left }),
                    ValueType.Bool      => Bool.Construct(new() { left }),
                    ValueType.Number    => Number.Construct(new() { left }),
                    ValueType.Range     => Values.Range.Construct(new() { left }),
                    ValueType.String    => String.Construct(new() { left }),
                    ValueType.Array     => Array.Construct(new() { left }),
                    ValueType.Struct    => Struct.Construct(new() { left }),
                    ValueType.Tuple     => Tuple.Construct(new() { left }),
                    ValueType.Func      => Func.Construct(new() { left }),
                    ValueType.Task      => Task.Construct(new() { left }, call),
                    ValueType.Iter      => Iter.Construct(new() { left }, call),
                    ValueType.Reference => Reference.Construct(new() { left }, call),
                    ValueType.Extern    => Extern.Construct(new() { left }),
                    ValueType.Type      => Type.Construct(new() { left }),

                    _ => throw new System.Exception()
                };
            }
            catch
            {
                return Null.Value;
            }
        }
    }
}