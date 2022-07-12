using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions
{
    internal class StrucLiteral : IExpression
    {
        private readonly Dictionary<string, IExpression> _lines;

        internal StrucLiteral(Dictionary<string, IExpression> lines)
        {
            _lines = lines;
        }

        public IPointer Evaluate(Call call)
        {
            var values = new Dictionary<string, IVariable>(_lines.Count);

            foreach (var (key, value) in _lines)
                values.Add(key, value.Evaluate(call).Value);

            return new Struct(values);
        }
    }
}