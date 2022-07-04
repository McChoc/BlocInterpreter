using System.Collections.Generic;
using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    internal class StrucLiteral : IExpression
    {
        private readonly Dictionary<string, IExpression> _lines;

        internal StrucLiteral(Dictionary<string, IExpression> lines)
        {
            _lines = lines;
        }

        public IValue Evaluate(Call call)
        {
            var values = new Dictionary<string, IValue>();

            foreach (var (key, value) in _lines)
                values.Add(key, value.Evaluate(call).Value);

            return new Struct(values);
        }
    }
}