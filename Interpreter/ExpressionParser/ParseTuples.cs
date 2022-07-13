using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Tokens;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseTuples(List<Token> tokens, int precedence)
        {
            var parts = tokens.Split(x => x is (TokenType.Operator, ","));

            if (parts.Count == 1)
                return Parse(tokens, precedence - 1);

            var expressions = new List<IExpression>();

            foreach (var part in parts)
                expressions.Add(Parse(part, precedence - 1));

            return new TupleLiteral(expressions);
        }
    }
}