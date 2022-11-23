using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions
{
    internal sealed class ArrayLiteral : IExpression
    {
        private readonly List<IExpression> _elements;

        internal ArrayLiteral(List<IExpression> elements)
        {
            _elements = elements;
        }

        public IPointer Evaluate(Call call)
        {
            var values = new List<IVariable>(_elements.Count);

            foreach (var expression in _elements)
                values.Add(expression.Evaluate(call).Value);

            return new Array(values);
        }

        public override bool Equals(object obj)
        {
            if (obj is not ArrayLiteral array)
                return false;

            if (!_elements.SequenceEqual(array._elements))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}