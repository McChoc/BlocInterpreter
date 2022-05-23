using CmmInterpretor.Memory;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Expressions
{
    public class StrucLiteral : IExpression
    {
        private readonly Dictionary<string, IExpression> _lines;

        public StrucLiteral(Dictionary<string, IExpression> lines)
        {
            _lines = lines;
        }

        public IValue Evaluate(Call call)
        {
            var values = new Dictionary<string, IValue>();

            foreach (var pair in _lines)
                values.Add(pair.Key, pair.Value.Evaluate(call).Value);

            return new Struct(values);
        }
    }
}
