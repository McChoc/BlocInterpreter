using System.Collections.Generic;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Scanners;
using Bloc.Tokens;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Eval : IExpression
    {
        private readonly IExpression _operand;

        internal Eval(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is not String @string)
                throw new Throw($"Cannot apply operator 'eval' on type {value.GetType().ToString().ToLower()}");

            var tokens = new List<Token>();
            var scanner = new TokenScanner(@string.Value);

            try
            {
                while (scanner.HasNextToken())
                    tokens.Add(scanner.GetNextToken());

                var expression = ExpressionParser.Parse(tokens);

                return expression.Evaluate(call);
            }
            catch (SyntaxError e)
            {
                throw new Throw(e.Text);
            }
            catch
            {
                throw new Throw("Failed to evaluate expression");
            }
        }
    }
}