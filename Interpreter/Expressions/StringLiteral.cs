using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed class StringLiteral : IExpression
    {
        private readonly string _baseString;
        private readonly List<(int, IExpression)> _expressions;

        internal StringLiteral(string baseString, List<(int, IExpression)> expressions)
        {
            _baseString = baseString;
            _expressions = expressions;
        }

        public IPointer Evaluate(Call call)
        {
            var offset = 0;

            var builder = new StringBuilder(_baseString);

            foreach (var (index, expression) in _expressions)
            {
                var value = expression.Evaluate(call).Value;

                var @string = String.ImplicitCast(value);

                builder.Insert(index + offset, @string.Value);
                offset += @string.Value.Length;
            }

            return new String(builder.ToString());
        }

        public override bool Equals(object obj)
        {
            if (obj is not StringLiteral @string)
                return false;

            if (_baseString != @string._baseString)
                return false;

            if (!_expressions.SequenceEqual(@string._expressions))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_baseString);
        }
    }
}