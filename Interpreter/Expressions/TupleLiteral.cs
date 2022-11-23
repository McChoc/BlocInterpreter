using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed class TupleLiteral : IExpression
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

        public override bool Equals(object obj)
        {
            if (obj is not TupleLiteral tuple)
                return false;

            if (!_expressions.SequenceEqual(tuple._expressions))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}