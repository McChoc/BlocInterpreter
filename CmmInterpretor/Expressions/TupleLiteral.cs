using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Utils;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Expressions
{
    public class TupleLiteral : IExpression
    {
        private readonly List<IExpression> _expressions;

        public TupleLiteral(List<IExpression> expressions)
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
