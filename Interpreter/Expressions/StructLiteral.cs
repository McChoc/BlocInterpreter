using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Expressions
{
    internal sealed class StructLiteral : IExpression
    {
        private readonly Dictionary<string, IExpression> _fields;

        internal StructLiteral(Dictionary<string, IExpression> fields)
        {
            _fields = fields;
        }

        public IPointer Evaluate(Call call)
        {
            var values = new Dictionary<string, IVariable>(_fields.Count);

            foreach (var (key, value) in _fields)
                values.Add(key, value.Evaluate(call).Value);

            return new Struct(values);
        }

        public override bool Equals(object obj)
        {
            if (obj is not StructLiteral @struct)
                return false;

            if (!_fields.SequenceEqual(@struct._fields))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}