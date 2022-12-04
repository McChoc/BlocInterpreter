using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed class StructLiteral : IExpression
    {
        private readonly Dictionary<string, IExpression> _fields;

        internal StructLiteral(Dictionary<string, IExpression> fields)
        {
            _fields = fields;
        }

        public IValue Evaluate(Call call)
        {
            var values = new Dictionary<string, Value>(_fields.Count);

            foreach (var (key, value) in _fields)
                values.Add(key, value.Evaluate(call).Value);

            return new Struct(values);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_fields.Count);
        }

        public override bool Equals(object other)
        {
            if (other is not StructLiteral literal)
                return false;

            if (_fields.Count != literal._fields.Count)
                return false;

            foreach (var key in _fields.Keys)
            {
                if (!literal._fields.TryGetValue(key, out var value))
                    return false;

                if (!_fields[key].Equals(value))
                    return false;
            }

            return true;
        }
    }
}