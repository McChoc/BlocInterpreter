using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseAssignments : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseAssignments(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            if (IsAssignment(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of assignment");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of assignment");

                var left = _nextStep.Parse(tokens.GetRange(..i));
                var right = Parse(tokens.GetRange((i + 1)..));

                return @operator.Text switch
                {
                    Symbol.EQUAL => new AssignmentOperator(left, right),
                    Symbol.PLUS_EQ => new AdditionAssignment(left, right),
                    Symbol.MINUS_EQ => new SubstractionAssignment(left, right),
                    Symbol.STAR_EQ => new MultiplicationAssignment(left, right),
                    Symbol.SLASH_EQ => new DivisionAssignment(left, right),
                    Symbol.PERCENT_EQ => new RemainderAssignment(left, right),
                    Symbol.DBL_PERCENT_EQ => new ModuloAssignment(left, right),
                    Symbol.DBL_STAR_EQ => new PowerAssignment(left, right),
                    Symbol.DBL_AMP_EQ => new BooleanAndAssignment(left, right),
                    Symbol.DBL_BAR_EQ => new BooleanOrAssignment(left, right),
                    Symbol.DBL_FLEX_EQ => new BooleanXorAssignment(left, right),
                    Symbol.AMP_EQ => new BitwiseAndAssignment(left, right),
                    Symbol.BAR_EQ => new BitwiseOrAssignment(left, right),
                    Symbol.FLEX_EQ => new BitwiseXorAssignment(left, right),
                    Symbol.L_SHIFT_EQ => new LeftShiftAssignment(left, right),
                    Symbol.R_SHIFT_EQ => new RightShiftAssignment(left, right),
                    Symbol.DBL_QUESTION_EQ => new NullCoalescingAssignment(left, right),
                    _ => throw new Exception()
                };
            }
        }

        return _nextStep.Parse(tokens);
    }

    private static bool IsAssignment(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is SymbolToken(
            Symbol.EQUAL or
            Symbol.PLUS_EQ or
            Symbol.MINUS_EQ or
            Symbol.STAR_EQ or
            Symbol.SLASH_EQ or
            Symbol.PERCENT_EQ or
            Symbol.DBL_PERCENT_EQ or
            Symbol.DBL_STAR_EQ or
            Symbol.DBL_AMP_EQ or
            Symbol.DBL_BAR_EQ or
            Symbol.DBL_FLEX_EQ or
            Symbol.AMP_EQ or
            Symbol.BAR_EQ or
            Symbol.FLEX_EQ or
            Symbol.L_SHIFT_EQ or
            Symbol.R_SHIFT_EQ or
            Symbol.DBL_QUESTION_EQ))
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