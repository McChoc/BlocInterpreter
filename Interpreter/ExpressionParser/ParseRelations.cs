using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators.Collection;
using Bloc.Operators.Relation;
using Bloc.Operators.Type;
using Bloc.Tokens;
using Bloc.Utils.Exceptions;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseRelations(List<Token> tokens, int precedence)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i] is (TokenType.Operator or TokenType.Keyword,
                    "<" or "<=" or ">" or ">=" or "in" or "not in" or "is" or "is not" or "as") @operator)
                {
                    if (i == 0)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of relation");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of relation");

                    var left = ParseRelations(tokens.GetRange(..i), precedence);
                    var right = Parse(tokens.GetRange((i + 1)..), precedence - 1);

                    return @operator.Text switch
                    {
                        "<" => new Less(left, right),
                        "<=" => new LessEqual(left, right),
                        ">" => new Greater(left, right),
                        ">=" => new GreaterEqual(left, right),
                        "in" => new In(left, right),
                        "not in" => new NotIn(left, right),
                        "is" => new Is(left, right),
                        "is not" => new IsNot(left, right),
                        "as" => new As(left, right),
                        _ => throw new Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}