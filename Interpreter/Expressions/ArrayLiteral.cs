using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions
{
    internal class ArrayLiteral : IExpression
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
    }
}