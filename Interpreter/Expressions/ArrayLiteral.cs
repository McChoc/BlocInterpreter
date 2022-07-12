using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions
{
    internal class ArrayLiteral : IExpression
    {
        private readonly List<IExpression> _expressions;

        internal ArrayLiteral(List<IExpression> expressions)
        {
            _expressions = expressions;
        }

        public IPointer Evaluate(Call call)
        {
            var values = new List<IVariable>(_expressions.Count);

            foreach (var expression in _expressions)
                values.Add(expression.Evaluate(call).Value);

            return new Array(values);
        }
    }
}