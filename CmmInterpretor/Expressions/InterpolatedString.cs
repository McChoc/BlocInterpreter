using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;
using System.Text;

namespace CmmInterpretor.Expressions
{
    public class InterpolatedString : IExpression
    {
        private readonly string _baseString;
        private readonly List<(int, IExpression)> _expressions;

        public InterpolatedString(string baseString, List<(int, IExpression)> expressions)
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

                if (!value!.Is(out String? str))
                    throw new Throw("Cannot implicitly convert to string");

                builder.Insert(index + offset, str!.Value);
                offset += str.Value.Length;
            }

            return new String(builder.ToString());
        }
    }
}
