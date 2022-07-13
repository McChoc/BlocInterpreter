using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class TupleLiteral : IExpression
    {
        private readonly List<IExpression> _expressions;

        internal TupleLiteral(List<IExpression> expressions)
        {
            _expressions = expressions;
        }

        public IPointer Evaluate(Call call)
        {
            var values = new List<IPointer>(_expressions.Count);

            foreach (var expression in _expressions)
                values.Add(expression.Evaluate(call));

            return new Tuple(values);
        }
    }
}