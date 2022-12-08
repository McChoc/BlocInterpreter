using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed class ArrayLiteral : IExpression
    {
        private readonly List<SubExpression> _expressions;

        internal ArrayLiteral(List<SubExpression> expressions)
        {
            _expressions = expressions;
        }

        public IValue Evaluate(Call call)
        {
            var values = new List<Value>();

            foreach (var expression in _expressions)
            {
                var value = expression.Expression.Evaluate(call).Value;

                if (!expression.Unpack)
                    values.Add(value);
                else if (value is Array array)
                    values.AddRange(array.Variables.Select(x => x.Value));
                else
                    throw new Throw("Only an array can be unpacked using the array unpack syntax");
            }

            return new Array(values);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_expressions.Count);
        }

        public override bool Equals(object other)
        {
            return other is ArrayLiteral literal &&
                _expressions.SequenceEqual(literal._expressions);
        }

        internal record SubExpression
        {
            internal bool Unpack { get; }
            internal IExpression Expression { get; }

            internal SubExpression(bool unpack, IExpression expression)
            {
                Unpack = unpack;
                Expression = expression;
            }
        }
    }
}