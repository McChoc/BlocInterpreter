using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Extensions;
using Bloc.Operators.Assignment;
using Bloc.Tokens;
using Bloc.Utils.Exceptions;

namespace Bloc
{
    internal static partial class ExpressionParser
    {
        private static IExpression ParseAssignments(List<Token> tokens, int precedence)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] is (TokenType.Operator,
                    "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "**=" or "//=" or "%%=" or
                    "&&=" or "||=" or "^^=" or "&=" or "|=" or "^=" or "<<=" or ">>=") @operator)
                {
                    if (i == 0)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of assignment");

                    if (i > tokens.Count - 1)
                        throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of assignment");

                    var left = Parse(tokens.GetRange(..i), precedence - 1);
                    var right = ParseAssignments(tokens.GetRange((i + 1)..), precedence);

                    return @operator.Text switch
                    {
                        "=" => new Assignment(left, right),
                        "+=" => new AdditionAssignment(left, right),
                        "-=" => new SubstractionAssignment(left, right),
                        "*=" => new MultiplicationAssignment(left, right),
                        "/=" => new DivisionAssignment(left, right),
                        "%=" => new RemainderAssignment(left, right),
                        "**=" => new PowerAssignment(left, right),
                        "//=" => new RootAssignment(left, right),
                        "%%=" => new LogarithmAssignment(left, right),
                        "&&=" => new BooleanAndAssignment(left, right),
                        "||=" => new BooleanOrAssignment(left, right),
                        "^^=" => new BooleanXorAssignment(left, right),
                        "&=" => new BitwiseAndAssignment(left, right),
                        "|=" => new BitwiseOrAssignment(left, right),
                        "^=" => new BitwiseXorAssignment(left, right),
                        "<<=" => new LeftShiftAssignment(left, right),
                        ">>=" => new RightShiftAssignment(left, right),
                        _ => throw new Exception()
                    };
                }
            }

            return Parse(tokens, precedence - 1);
        }
    }
}