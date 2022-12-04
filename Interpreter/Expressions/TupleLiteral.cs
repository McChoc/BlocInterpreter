using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions
{
    internal sealed class TupleLiteral : IExpression
    {
        private readonly List<IExpression> _expressions;

        internal TupleLiteral(List<IExpression> expressions)
        {
            _expressions = expressions;
        }

        public IValue Evaluate(Call call)
        {
            var variables = new List<IVariable>(_expressions.Count);

            foreach (var expression in _expressions)
            {
                var value = expression.Evaluate(call);

                if (value is IVariable variable)
                    variables.Add(variable);
                else
                    variables.Add(new TupleVariable(value.Value));
            }

            return new Tuple(variables);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_expressions.Count);
        }

        public override bool Equals(object other)
        {
            return other is TupleLiteral literal &&
                _expressions.SequenceEqual(literal._expressions);
        }
    }
}