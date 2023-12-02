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
using Bloc.Utils.Helpers;

namespace Bloc.Parsers.Steps;

internal sealed class ParseRelations : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParseRelations(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<IToken> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (IsTypeTest(tokens[i], out var @operator))
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of relation");

                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of relation");

                var left = Parse(tokens.GetRange(..i));
                var right = _nextStep.Parse(tokens.GetRange((i + 1)..));

                return @operator.Text switch
                {
                    Keyword.NOT_IN => new NotInOperator(left, right),
                    Keyword.IS => new IsOperator(left, right),
                    Keyword.IS_NOT => new IsNotOperator(left, right),
                    Keyword.AS => new AsOperator(left, right),
                    _ => throw new Exception()
                };
            }
            
            if (IsRelation(tokens[i], out @operator))
            {
                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of relation");

                if (OperatorHelper.IsBinary(tokens, i))
                {
                    var left = Parse(tokens.GetRange(..i));
                    var right = _nextStep.Parse(tokens.GetRange((i + 1)..));

                    return @operator.Text switch
                    {
                        Symbol.LESS => new LessThanOperator(left, right),
                        Symbol.LESS_EQ => new LessEqualOperator(left, right),
                        Symbol.MORE => new GreaterThanOperator(left, right),
                        Symbol.MORE_EQ => new GreaterEqualOperator(left, right),
                        Keyword.IN => new InOperator(left, right),
                        Keyword.NOT_IN => new NotInOperator(left, right),
                        _ => throw new Exception()
                    };
                }
                else
                {
                    var operand = _nextStep.Parse(tokens.GetRange((i + 1)..));

                    IExpression expression = @operator.Text switch
                    {
                        Symbol.LESS => new LessThanPatternLiteral(operand),
                        Symbol.LESS_EQ => new LessEqualPatternLiteral(operand),
                        Symbol.MORE => new GreaterThanPatternLiteral(operand),
                        Symbol.MORE_EQ => new GreaterEqualPatternLiteral(operand),
                        Keyword.IN => new InPatternLiteral(operand),
                        Keyword.NOT_IN => new NotInPatternLiteral(operand),
                        _ => throw new Exception()
                    };

                    var parsedToken = new ParsedToken(tokens[i].Start, tokens[^1].End, expression);

                    tokens.RemoveRange(i..);
                    tokens.Add(parsedToken);

                    return Parse(tokens);
                }
            }
        }

        return _nextStep.Parse(tokens);
    }

    private static bool IsTypeTest(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is KeywordToken(Keyword.IS or Keyword.IS_NOT or Keyword.AS))
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

    private static bool IsRelation(IToken token, [NotNullWhen(true)] out TextToken? @operator)
    {
        if (token is
            SymbolToken(Symbol.LESS or Symbol.LESS_EQ or Symbol.MORE or Symbol.MORE_EQ) or
            KeywordToken(Keyword.IN or Keyword.NOT_IN))
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