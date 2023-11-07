using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Expressions.Patterns;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseUnaries : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseUnaries(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Missing value");

        if (IsDeclaration(tokens[0], out var @operator))
        {
            var identifier = IdentifierParser.Parse(tokens.GetRange(1..));

            return @operator.Text switch
            {
                Keyword.LET     => new LetOperator(identifier),
                Keyword.LET_NEW => new LetNewOperator(identifier),
                _ => throw new Exception()
            };
        }

        if (IsPattern(tokens[0], out @operator))
        {
            var operand = Parse(tokens.GetRange(1..));

            return @operator.Text switch
            {
                Symbol.IS_EQUAL => new EqualPatternLiteral(operand),
                Symbol.MORE_THAN => new GreaterThanPatternLiteral(operand),
                Symbol.MORE_EQUAL => new GreaterEqualPatternLiteral(operand),
                Symbol.LESS_THAN => new LessThanPatternLiteral(operand),
                Symbol.LESS_EQUAL => new LessEqualPatternLiteral(operand),
                Keyword.IN => new InPatternLiteral(operand),
                Keyword.NOT_IN => new NotInPatternLiteral(operand),
                _ => new NotEqualPatternLiteral(operand)
            };
        }

        if (IsPrefix(tokens[0], out @operator))
        {
            var operand = Parse(tokens.GetRange(1..));

            return @operator.Text switch
            {
                Symbol.PLUS         => new PositiveOperator(operand),
                Symbol.MINUS        => new NegativeOperator(operand),
                Symbol.BIT_NOT      => new ComplementOperator(operand),
                Symbol.BOOL_NOT     => new NegationOperator(operand),
                Symbol.INCREMENT    => new IncrementPrefix(operand),
                Symbol.DECREMENT    => new DecrementPrefix(operand),
                Symbol.BIT_INV      => new ComplementPrefix(operand),
                Symbol.BOOL_INV     => new NegationPrefix(operand),
                Keyword.LEN         => new LengthOperator(operand),
                Keyword.CHR         => new ChrOperator(operand),
                Keyword.ORD         => new OrdOperator(operand),
                Keyword.REF         => new RefOperator(operand),
                Keyword.VAL         => new ValOperator(operand),
                Keyword.LVAL        => new LvalOperator(operand),
                Keyword.RVAL        => new RvalOperator(operand),
                Keyword.NEW         => new NewOperator(operand),
                Keyword.CONST_NEW   => new ConstNewOperator(operand),
                Keyword.DELETE      => new DeleteOperator(operand),
                Keyword.AWAIT       => new AwaitOperator(operand),
                Keyword.NEXT        => new NextOperator(operand),
                Keyword.NAMEOF      => new NameofOperator(operand),
                Keyword.TYPEOF      => new TypeofOperator(operand),
                Keyword.EVAL        => new EvalOperator(operand),
                _ => throw new Exception()
            };
        }

        if (IsPostfix(tokens[^1], out @operator))
        {
            var operand = Parse(tokens.GetRange(..^1));

            return @operator.Text switch
            {
                Symbol.INCREMENT    => new IncrementPostfix(operand),
                Symbol.DECREMENT    => new DecrementPostfix(operand),
                Symbol.BIT_INV      => new ComplementPostfix(operand),
                Symbol.BOOL_INV     => new NegationPostfix(operand),
                _ => throw new Exception()
            };
        }

        return _nextStep.Parse(tokens);
    }

    private static bool IsDeclaration(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is KeywordToken(Keyword.LET or Keyword.LET_NEW))
        {
            @operator = (TextToken)token;
            return true;
        }

        @operator = null;
        return false;
    }

    private static bool IsPattern(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is KeywordToken(Keyword.IN or Keyword.NOT_IN))
        {
            @operator = (TextToken)token;
            return true;
        }

        if (token is SymbolToken(
            Symbol.IS_EQUAL or
            Symbol.NOT_EQUAL_0 or
            Symbol.NOT_EQUAL_1 or
            Symbol.NOT_EQUAL_2 or
            Symbol.MORE_THAN or
            Symbol.MORE_EQUAL or
            Symbol.LESS_THAN or
            Symbol.LESS_EQUAL))
        {
            @operator = (TextToken)token;
            return true;
        }

        @operator = null;
        return false;
    }

    private static bool IsPostfix(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is SymbolToken(
                Symbol.INCREMENT or
                Symbol.DECREMENT or
                Symbol.BIT_INV or
                Symbol.BOOL_INV))
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

    private static bool IsPrefix(IToken token, [NotNullWhen(true)] out TextToken? @operator)
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
            Keyword.LVAL or
            Keyword.RVAL or
            Keyword.NEW or
            Keyword.CONST_NEW or
            Keyword.DELETE or
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
}