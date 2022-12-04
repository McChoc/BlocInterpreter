using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed class ArrayLiteral : IExpression
    {
        private readonly List<IExpression> _elements;

        internal ArrayLiteral(List<IExpression> elements)
        {
            _elements = elements;
        }

        public IValue Evaluate(Call call)
        {
            var values = new List<Value>(_elements.Count);

            foreach (var expression in _elements)
                values.Add(expression.Evaluate(call).Value);

            return new Array(values);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_elements.Count);
        }

        public override bool Equals(object other)
        {
            return other is ArrayLiteral literal &&
                _elements.SequenceEqual(literal._elements);
        }
    }
}