using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions
{
    internal class StructLiteral : IExpression
    {
        private readonly Dictionary<string, IExpression> _properties;

        internal StructLiteral(Dictionary<string, IExpression> properties)
        {
            _properties = properties;
        }

        public IPointer Evaluate(Call call)
        {
            var values = new Dictionary<string, IVariable>(_properties.Count);

            foreach (var (key, value) in _properties)
                values.Add(key, value.Evaluate(call).Value);

            return new Struct(values);
        }
    }
}