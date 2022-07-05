using System.Collections.Generic;
using Bloc.Memory;
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

        public IValue Evaluate(Call call)
        {
            var values = new List<IValue>();

            foreach (var expression in _expressions)
                values.Add(expression.Evaluate(call));

            return new Tuple(values);
        }
    }
}