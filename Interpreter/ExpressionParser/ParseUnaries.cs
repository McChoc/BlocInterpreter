using System;
using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Operators;
using Bloc.Tokens;
using Bloc.Utils.Extensions;
using Nullable = Bloc.Operators.Nullable;

namespace Bloc;

internal static partial class ExpressionParser
{
    private static IExpression ParseUnaries(List<Token> tokens, int precedence)
    {
        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Missing value");

        if (IsPrefix(tokens[0], out var prefix))
        {
            var operand = ParseUnaries(tokens.GetRange(1..), precedence);

            return prefix!.Text switch
            {
                Symbol.PLUS         => new Plus(operand),
                Symbol.MINUS        => new Minus(operand),
                Symbol.BIT_NOT      => new Complement(operand),
                Symbol.BOOL_NOT     => new Negation(operand),
                Symbol.INCREMENT    => new PreIncrement(operand),
                Symbol.DECREMENT    => new PreDecrement(operand),
                Symbol.BIT_INV      => new PreComplement(operand),
                Symbol.BOOL_INV     => new PreNegation(operand),
                Keyword.LEN         => new Length(operand),
                Keyword.CHR         => new Character(operand),
                Keyword.ORD         => new Ordinal(operand),
                Keyword.REF         => new Ref(operand),
                Keyword.VAL         => new Val(operand),
                Keyword.VAL_VAL     => new ValVal(operand),
                Keyword.LET         => new Let(operand),
                Keyword.LET_NEW     => new LetNew(operand),
                Keyword.NEW         => new New(operand),
                Keyword.CONST_NEW   => new ConstNew(operand),
                Keyword.DELETE      => new Delete(operand),
                Keyword.GLOBAL      => new Global(operand),
                Keyword.NONLOCAL    => new Nonlocal(operand),
                Keyword.PARAM       => new Param(operand),
                Keyword.AWAIT       => new Await(operand),
                Keyword.NEXT        => new Next(operand),
                Keyword.NAMEOF      => new Nameof(operand),
                Keyword.TYPEOF      => new Typeof(operand),
                Keyword.EVAL        => new Eval(operand),
                _ => throw new Exception()
            };
        }

        if (IsPostfix(tokens[^1], out var postfix))
        {
            var operand = ParseUnaries(tokens.GetRange(..^1), precedence);

            return postfix!.Text switch
            {
                Symbol.INCREMENT    => new PostIncrement(operand),
                Symbol.DECREMENT    => new PostDecrement(operand),
                Symbol.BIT_INV      => new PostComplement(operand),
                Symbol.BOOL_INV     => new PostNegation(operand),
                Symbol.QUESTION     => new Nullable(operand),
                _ => throw new Exception()
            };
        }

        return Parse(tokens, precedence - 1);
    }

    private static bool IsPrefix(Token token, out TextToken? @operator)
    {
        if (token is SymbolToken(
            Symbol.PLUS or
            Symbol.MINUS or
            Symbol.BIT_NOT or
            Symbol.BOOL_NOT or
            Symbol.INCREMENT or
            Symbol.DECREMENT or
            Symbol.BIT_INV or
            Symbol.BOOL_INV))
        {
            @operator = (TextToken)token;
            return true;
        }

        if (token is KeywordToken(
            Keyword.LEN or
            Keyword.CHR or
            Keyword.ORD or
            Keyword.REF or
            Keyword.VAL or
            Keyword.VAL_VAL or
            Keyword.LET or
            Keyword.LET_NEW or
            Keyword.NEW or
            Keyword.CONST_NEW or
            Keyword.DELETE or
            Keyword.GLOBAL or
            Keyword.NONLOCAL or
            Keyword.PARAM or
            Keyword.AWAIT or
            Keyword.NEXT or
            Keyword.NAMEOF or
            Keyword.TYPEOF or
            Keyword.EVAL))
        {
            @operator = (TextToken)token;
            return true;
        }

        @operator = null;
        return false;
    }

    private static bool IsPostfix(Token token, out TextToken? @operator)
    {
        if (token is SymbolToken(
                Symbol.INCREMENT or
                Symbol.DECREMENT or
                Symbol.BIT_INV or
                Symbol.BOOL_INV or
                Symbol.QUESTION))
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