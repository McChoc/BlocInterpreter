using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParsePrimaries : IParsingStep
{
    private readonly IParsingStep _nextStep;

    public ParsePrimaries(IParsingStep nextStep)
    {
        _nextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        var expression = _nextStep.Parse(tokens.GetRange(0, 1));

        for (int i = 1; i < tokens.Count; i++)
        {
            if (tokens[i] is SymbolToken(Symbol.ACCESS_MEMBER))
            {
                if (tokens.Count <= i)
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                if (tokens[i + 1] is not INamedIdentifierToken token)
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                var identifier = token.GetIdentifier();

                expression = new MemberAccessOperator(expression, identifier);

                i++;
            }
            else if (tokens[i] is BracketsToken brackets)
            {
                var index = ExpressionParser.Parse(brackets.Tokens);

                expression = new IndexerOperator(expression, index);
            }
            else if (tokens[i] is ParenthesesToken parentheses)
            {
                var content = parentheses.Tokens;

                var arguments = new List<InvocationOperator.Argument>();

                if (content.Count > 0)
                {
                    foreach (var part in content.Split(x => x is SymbolToken(Symbol.COMMA)))
                    {
                        if (part.Count == 0)
                        {
                            if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                                throw new SyntaxError(0, 0, "All the positional arguments must apear before any named arguments");

                            arguments.Add(new(null, new VoidLiteral(), InvocationOperator.ArgumentType.Positional));
                        }
                        else if (part[0] is SymbolToken(Symbol.UNPACK_ARRAY))
                        {
                            if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                                throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                            arguments.Add(new(null, ExpressionParser.Parse(part.GetRange(1..)), InvocationOperator.ArgumentType.UnpackedArray));
                        }
                        else if (part[0] is SymbolToken(Symbol.UNPACK_STRUCT))
                        {
                            arguments.Add(new(null, ExpressionParser.Parse(part.GetRange(1..)), InvocationOperator.ArgumentType.UnpackedStruct));
                        }
                        else
                        {
                            int index = part.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

                            if (index == -1)
                            {
                                if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                                    throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                                arguments.Add(new(null, ExpressionParser.Parse(part), InvocationOperator.ArgumentType.Positional));
                            }
                            else
                            {
                                var keyTokens = part.GetRange(..index);
                                var valueTokens = part.GetRange((index + 1)..);

                                if (keyTokens.Count == 0)
                                    throw new SyntaxError(0, 0, "Missing identifier");

                                var keyExpr = ExpressionParser.Parse(keyTokens);

                                if (keyExpr is not NamedIdentifierExpression identifier)
                                    throw new SyntaxError(0, 0, "Invalid identifier");

                                if (valueTokens.Count == 0)
                                    throw new SyntaxError(0, 0, "Missing value");

                                arguments.Add(new(identifier.Identifier, ExpressionParser.Parse(valueTokens), InvocationOperator.ArgumentType.Named));
                            }
                        }
                    }
                }

                expression = new InvocationOperator(expression, arguments);
            }
            else
            {
                throw new SyntaxError(tokens[i].Start, tokens[i].End, "Unexpected symbol");
            }
        }

        return expression;
    }
}