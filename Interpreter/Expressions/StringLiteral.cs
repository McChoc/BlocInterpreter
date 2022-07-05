using System.Collections.Generic;
using System.Text;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class StringLiteral : IExpression
    {
        private readonly string _baseString;
        private readonly List<(int, IExpression)> _expressions;

        internal StringLiteral(string baseString, List<(int, IExpression)> expressions)
        {
            _baseString = baseString;
            _expressions = expressions;
        }

        public IValue Evaluate(Call call)
        {
            var offset = 0;

            var builder = new StringBuilder(_baseString);

            foreach (var (index, expression) in _expressions)
            {
                var value = expression.Evaluate(call);

                if (!value.Is(out String? str))
                    throw new Throw("Cannot implicitly convert to string");

                builder.Insert(index + offset, str!.Value);
                offset += str.Value.Length;
            }

            return new String(builder.ToString());
        }
    }
}