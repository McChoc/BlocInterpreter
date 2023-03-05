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
    private static IExpression ParsePrimaries(List<Token> tokens, int precedence)
    {
        var expression = Parse(tokens.GetRange(0, 1), precedence - 1);

        for (var i = 1; i < tokens.Count; i++)
        {
            if (tokens[i] is SymbolToken(Symbol.ACCESS_MEMBER))
            {
                if (tokens.Count <= i)
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                if (tokens[i + 1] is not IIdentifierToken identifier)
                    throw new SyntaxError(tokens[i].Start, tokens[i].End, "Missing identifier");

                expression = new MemberAccessOperator(expression, identifier.Text);

                i++;
            }
            else if (tokens[i] is IndexerToken indexer)
            {
                var index = Parse(indexer.Tokens);

                expression = new IndexerOperator(expression, index);
            }
            else if (tokens[i] is GroupToken group)
            {
                var content = group.Tokens;

                var arguments = new List<InvocationOperator.Argument>();

                if (content.Count > 0)
                {
                    foreach (var part in content.Split(x => x is SymbolToken(Symbol.COMMA)))
                    {
                        if (part.Count == 0)
                        {
                            if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                                throw new SyntaxError(0, 0, "All the positional arguments must apear before any named arguments");

                            arguments.Add(new(null, InvocationOperator.ArgumentType.Positional, new VoidLiteral()));
                        }
                        else if (part[0] is SymbolToken(Symbol.UNPACK_ARRAY))
                        {
                            if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                                throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                            arguments.Add(new(null, InvocationOperator.ArgumentType.UnpackedArray, Parse(part.GetRange(1..))));
                        }
                        else if (part[0] is SymbolToken(Symbol.UNPACK_STRUCT))
                        {
                            arguments.Add(new(null, InvocationOperator.ArgumentType.UnpackedStruct, Parse(part.GetRange(1..))));
                        }
                        else
                        {
                            var index = part.FindIndex(x => x is SymbolToken(Symbol.ASSIGN));

                            if (index == -1)
                            {
                                if (arguments.Count > 0 && arguments[^1].Type is InvocationOperator.ArgumentType.Named or InvocationOperator.ArgumentType.UnpackedStruct)
                                    throw new SyntaxError(part[0].Start, part[^1].End, "All the positional arguments must apear before any named arguments");

                                arguments.Add(new(null, InvocationOperator.ArgumentType.Positional, Parse(part)));
                            }
                            else
                            {
                                var keyTokens = part.GetRange(..index);
                                var valueTokens = part.GetRange((index + 1)..);

                                if (keyTokens.Count == 0)
                                    throw new SyntaxError(0, 0, "Missing identifier");

                                var keyExpr = Parse(keyTokens);

                                if (keyExpr is not Identifier identifier)
                                    throw new SyntaxError(0, 0, "Invalid identifier");

                                if (valueTokens.Count == 0)
                                    throw new SyntaxError(0, 0, "Missing value");

                                arguments.Add(new(identifier.Name, InvocationOperator.ArgumentType.Named, Parse(valueTokens)));
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