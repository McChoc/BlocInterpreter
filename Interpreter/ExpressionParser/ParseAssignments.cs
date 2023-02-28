using System;
using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc;

internal static partial class ExpressionParser
{
    private static IExpression ParseAssignments(List<Token> tokens, int precedence)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            if (IsAssignment(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the left part of assignment");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator!.Start, @operator.End, "Missing the right part of assignment");

                var left = Parse(tokens.GetRange(..i), precedence - 1);
                var right = ParseAssignments(tokens.GetRange((i + 1)..), precedence);

                return @operator!.Text switch
                {
                    Symbol.ASSIGN           => new Assignment(left, right),
                    Symbol.ASSIGN_SUM       => new AdditionAssignment(left, right),
                    Symbol.ASSIGN_DIF       => new SubstractionAssignment(left, right),
                    Symbol.ASSIGN_PRODUCT   => new MultiplicationAssignment(left, right),
                    Symbol.ASSIGN_QUOTIENT  => new DivisionAssignment(left, right),
                    Symbol.ASSIGN_REMAINDER => new RemainderAssignment(left, right),
                    Symbol.ASSIGN_POWER     => new PowerAssignment(left, right),
                    Symbol.ASSIGN_ROOT      => new RootAssignment(left, right),
                    Symbol.ASSIGN_LOGARITHM => new LogarithmAssignment(left, right),
                    Symbol.ASSIGN_BOOL_AND  => new BooleanAndAssignment(left, right),
                    Symbol.ASSIGN_BOOL_OR   => new BooleanOrAssignment(left, right),
                    Symbol.ASSIGN_BOOL_XOR  => new BooleanXorAssignment(left, right),
                    Symbol.ASSIGN_BIT_AND   => new BitwiseAndAssignment(left, right),
                    Symbol.ASSIGN_BIT_OR    => new BitwiseOrAssignment(left, right),
                    Symbol.ASSIGN_BIT_XOR   => new BitwiseXorAssignment(left, right),
                    Symbol.ASSIGN_SHIFT_L   => new LeftShiftAssignment(left, right),
                    Symbol.ASSIGN_SHIFT_R   => new RightShiftAssignment(left, right),
                    Symbol.ASSIGN_COALESCE  => new NullCoalescingAssignment(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return Parse(tokens, precedence - 1);
    }

    private static bool IsAssignment(Token token, out TextToken? @operator)
    {
        if (token is SymbolToken(
            Symbol.ASSIGN or
            Symbol.ASSIGN_SUM or
            Symbol.ASSIGN_DIF or
            Symbol.ASSIGN_PRODUCT or
            Symbol.ASSIGN_QUOTIENT or
            Symbol.ASSIGN_REMAINDER or
            Symbol.ASSIGN_POWER or
            Symbol.ASSIGN_ROOT or
            Symbol.ASSIGN_LOGARITHM or
            Symbol.ASSIGN_BOOL_AND or
            Symbol.ASSIGN_BOOL_OR or
            Symbol.ASSIGN_BOOL_XOR or
            Symbol.ASSIGN_BIT_AND or
            Symbol.ASSIGN_BIT_OR or
            Symbol.ASSIGN_BIT_XOR or
            Symbol.ASSIGN_SHIFT_L or
            Symbol.ASSIGN_SHIFT_R or
            Symbol.ASSIGN_COALESCE))
        {
            @operator = (TextToken)token;
            return true;
        }
        else
        {
            @operator = null;
            return false;
        }
    }
}