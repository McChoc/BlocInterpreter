using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseUnaries : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseUnaries(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Missing value");

        if (NeedsNameIdentifier(tokens[0]))
        {
            var @operator = (TextToken)tokens[0];
            var identifier = IdentifierParser.ParseName(tokens.GetRange(1..));

            return @operator.Text switch
            {
                Keyword.DEF => new DefOperator(identifier),
                Keyword.NOT_DEF => new NotDefOperator(identifier),
                _ => throw new Exception()
            };
        }

        if (NeedsIdentifier(tokens[0]))
        {
            var @operator = (TextToken)tokens[0];
            var identifier = IdentifierParser.Parse(tokens.GetRange(1..));

            return @operator.Text switch
            {
                Keyword.LET => new LetOperator(identifier),
                Keyword.LET_NEW => new LetNewOperator(identifier),
                _ => throw new Exception()
            };
        }

        if (IsPrefix(tokens[0]))
        {
            var prefix = (TextToken)tokens[0];
            var operand = Parse(tokens.GetRange(1..));

            return prefix.Text switch
            {
                Symbol.PLUS         => new PositiveOperator(operand),
                Symbol.MINUS        => new NegativeOperator(operand),
                Symbol.TILDE        => new ComplementOperator(operand),
                Symbol.BOOL_NOT     => new NegationOperator(operand),
                Symbol.INCREMENT    => new IncrementPrefix(operand),
                Symbol.DECREMENT    => new DecrementPrefix(operand),
                Symbol.BIT_INV      => new ComplementPrefix(operand),
                Symbol.BOOL_INV     => new NegationPrefix(operand),
                Keyword.LEN         => new LengthOperator(operand),
                Keyword.CHR         => new CharacterOperator(operand),
                Keyword.ORD         => new OrdinalOperator(operand),
                Keyword.REF         => new RefOperator(operand),
                Keyword.VAL         => new ValOperator(operand),
                Keyword.VAL_VAL     => new ValValOperator(operand),
                Keyword.NEW         => new NewOperator(operand),
                Keyword.CONST_NEW   => new ConstNewOperator(operand),
                Keyword.DELETE      => new DeleteOperator(operand),
                Keyword.GLOBAL      => new GlobalOperator(operand),
                Keyword.NONLOCAL    => new NonlocalOperator(operand),
                Keyword.PARAM       => new ParamOperator(operand),
                Keyword.AWAIT       => new AwaitOperator(operand),
                Keyword.NEXT        => new NextOperator(operand),
                Keyword.NAMEOF      => new NameofOperator(operand),
                Keyword.TYPEOF      => new TypeofOperator(operand),
                Keyword.EVAL        => new EvalOperator(operand),
                _ => throw new Exception()
            };
        }

        if (IsPostfix(tokens[^1]))
        {
            var postfix = (TextToken)tokens[^1];
            var operand = Parse(tokens.GetRange(..^1));

            return postfix.Text switch
            {
                Symbol.INCREMENT    => new IncrementPostfix(operand),
                Symbol.DECREMENT    => new DecrementPostfix(operand),
                Symbol.BIT_INV      => new ComplementPostfix(operand),
                Symbol.BOOL_INV     => new NegationPostfix(operand),
                Symbol.QUESTION     => new NullableTypeOperator(operand),
                Symbol.TILDE        => new VoidableTypeOperator(operand),
                _ => throw new Exception()
            };
        }

        return NextStep!.Parse(tokens);
    }

    private static bool NeedsNameIdentifier(Token token)    
    {
        return token is KeywordToken(Keyword.DEF or Keyword.NOT_DEF);
    }

    private static bool NeedsIdentifier(Token token)
    {
        return token is KeywordToken(Keyword.LET or Keyword.LET_NEW);
    }

    private static bool IsPostfix(Token token)
    {
        return token is SymbolToken(
            Symbol.INCREMENT or
            Symbol.DECREMENT or
            Symbol.BIT_INV or
            Symbol.BOOL_INV or
            Symbol.QUESTION or
            Symbol.TILDE);
    }

    private static bool IsPrefix(Token token)
    {
        return token
            is SymbolToken(
                Symbol.PLUS or
                Symbol.MINUS or
                Symbol.TILDE or
                Symbol.BOOL_NOT or
                Symbol.INCREMENT or
                Symbol.DECREMENT or
                Symbol.BIT_INV or
                Symbol.BOOL_INV)
            or KeywordToken(
                Keyword.LEN or
                Keyword.CHR or
                Keyword.ORD or
                Keyword.REF or
                Keyword.VAL or
                Keyword.VAL_VAL or
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
                Keyword.EVAL);
    }
}