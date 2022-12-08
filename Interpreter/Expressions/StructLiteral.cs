using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed class StructLiteral : IExpression
    {
        private readonly List<SubExpression> _expressions;

        internal StructLiteral(List<SubExpression> expressions)
        {
            _expressions = expressions;
        }

        public IValue Evaluate(Call call)
        {
            var values = new Dictionary<string, Value>();

            foreach (var expression in _expressions)
            {
                var value = expression.Expression.Evaluate(call).Value;

                if (!expression.Unpack)
                    values.Add(expression.Name!, value);
                else if (value is Struct @struct)
                    foreach (var (key, variable) in @struct.Variables)
                        values.Add(key, variable.Value);
                else
                    throw new Throw("Only a struct can be unpacked using the struct unpack syntax");
            }

            return new Struct(values);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_expressions.Count);
        }

        public override bool Equals(object other)
        {
            return other is StructLiteral literal &&
                _expressions.SequenceEqual(literal._expressions);
        }

        internal record SubExpression
        {
            internal bool Unpack { get; }
            internal string? Name { get; }
            internal IExpression Expression { get; }

            internal SubExpression(bool unpack, string? name, IExpression expression)
            {
                Unpack = unpack;
                Name = name;
                Expression = expression;
            }
        }
    }
}