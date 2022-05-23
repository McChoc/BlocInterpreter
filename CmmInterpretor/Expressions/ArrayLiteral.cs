using CmmInterpretor.Memory;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Expressions
{
    public class ArrayLiteral : IExpression
    {
        private readonly List<IExpression> _expressions;

        public ArrayLiteral(List<IExpression> expressions)
        {
            _expressions = expressions;
        }

        public IValue Evaluate(Call call)
        {
            var values = new List<IValue>();

            foreach (var expression in _expressions)
                values.Add(expression.Evaluate(call).Value);

            return new Array(values);
        }
    }
}
