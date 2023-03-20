using System.Collections.Generic;
using System.Linq;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Expressions.Elements;
using Bloc.Expressions.Members;
using Bloc.Tokens;
using Bloc.Utils.Extensions;
using Bloc.Values;

namespace Bloc.Parsers.Steps;

internal sealed class ParseAtoms : IParsingStep
{
    public IParsingStep? NextStep { get; init; }

    public ParseAtoms(IParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    public IExpression Parse(List<Token> tokens)
    {
        return tokens[0] switch
        {
            IIdentifierToken identifier => new Identifier(identifier.Text),

            GroupToken group => ExpressionParser.Parse(group.Tokens),

            NumberToken number => ParseNumber(number),
            StringToken @string => ParseString(@string),
            ArrayToken array => ParseArray(array),
            StructToken @struct => ParseStruct(@struct),
            FuncToken func => func.Literal,

            LiteralToken(Keyword.VOID) => new VoidLiteral(),
            LiteralToken(Keyword.NULL) => new NullLiteral(),
            LiteralToken(Keyword.FALSE) => new BoolLiteral(false),
            LiteralToken(Keyword.TRUE) => new BoolLiteral(true),
            LiteralToken(Keyword.NAN) => new NumberLiteral(double.NaN),
            LiteralToken(Keyword.INFINITY) => new NumberLiteral(double.PositiveInfinity),

            LiteralToken(Keyword.VOID_T) => new TypeLiteral(ValueType.Void),
            LiteralToken(Keyword.NULL_T) => new TypeLiteral(ValueType.Null),
            LiteralToken(Keyword.BOOL) => new TypeLiteral(ValueType.Bool),
            LiteralToken(Keyword.NUMBER) => new TypeLiteral(ValueType.Number),
            LiteralToken(Keyword.RANGE) => new TypeLiteral(ValueType.Range),
            LiteralToken(Keyword.STRING) => new TypeLiteral(ValueType.String),
            LiteralToken(Keyword.ARRAY) => new TypeLiteral(ValueType.Array),
            LiteralToken(Keyword.STRUCT) => new TypeLiteral(ValueType.Struct),
            LiteralToken(Keyword.TUPLE) => new TypeLiteral(ValueType.Tuple),
            LiteralToken(Keyword.FUNC) => new TypeLiteral(ValueType.Func),
            LiteralToken(Keyword.TASK) => new TypeLiteral(ValueType.Task),
            LiteralToken(Keyword.ITER) => new TypeLiteral(ValueType.Iter),
            LiteralToken(Keyword.REFERENCE) => new TypeLiteral(ValueType.Reference),
            LiteralToken(Keyword.EXTERN) => new TypeLiteral(ValueType.Extern),
            LiteralToken(Keyword.TYPE) => new TypeLiteral(ValueType.Type),

            _ => throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected token")
        };
    }

    private static NumberLiteral ParseNumber(NumberToken token)
    {
        return new NumberLiteral(token.Number);
    }

    private static StringLiteral ParseString(StringToken token)
    {
        var interpolations = token.Interpolations
            .Select(x => new StringLiteral.Interpolation(x.Index, ExpressionParser.Parse(x.Tokens)))
            .ToList();

        return new StringLiteral(token.BaseString, interpolations);
    }

    private static ArrayLiteral ParseArray(ArrayToken token)
    {
        var parts = token.Tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        var elements = new List<IElement>();

        foreach (var part in parts)
        {
            if (part.Count == 0)
                throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

            if (part.Any(x => x is SymbolToken(Symbol.ASSIGN)))
                throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

            switch(part[0])
            {
                case SymbolToken(Symbol.UNPACK_STRUCT):
                    throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                case SymbolToken(Symbol.UNPACK_ARRAY):
                {
                    var tokens = part.GetRange(1..);

                    if (tokens.Count == 0)
                        continue;

                    var expression = ExpressionParser.Parse(tokens);
                    var element = new UnpackedElement(expression);
                    elements.Add(element);
                    break;
                }

                default:
                {
                    var expression = ExpressionParser.Parse(part);
                    var element = new Element(expression);
                    elements.Add(element);
                    break;
                }
            }
        }

        return new ArrayLiteral(elements);
    }

    private static StructLiteral ParseStruct(StructToken token)
    {
        var parts = token.Tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts[^1].Count == 0)
            parts.RemoveAt(parts.Count - 1);

        var members = new List<IMember>();

        foreach (var part in parts)
        {
            if (part.Count == 0)
                throw new SyntaxError(0, 0, $"Unexpected symbol '{Symbol.COMMA}'");

            switch (part[0])
            {
                case IIdentifierToken identifier:
                {
                    if (!part.Any(x => x is SymbolToken(Symbol.ASSIGN)))
                        throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                    if (part[1] is not SymbolToken(Symbol.ASSIGN))
                        throw new SyntaxError(part[1].Start, part[1].End, "Unexpected token.");

                    var tokens = part.GetRange(2..);

                    if (tokens.Count == 0)
                        throw new SyntaxError(0, 0, "Missing value.");

                    var expression = ExpressionParser.Parse(tokens);
                    var member = new StaticlyNamedMember(identifier.Text, expression);
                    members.Add(member);
                    break;
                }

                case IndexerToken indexer:
                {
                    if (!part.Any(x => x is SymbolToken(Symbol.ASSIGN)))
                        throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");

                    if (indexer.Tokens.Count == 0)
                        throw new SyntaxError(part[0].Start, part[0].End, "Missing value.");

                    var nameExpression = ExpressionParser.Parse(indexer.Tokens);

                    if (part[1] is not SymbolToken(Symbol.ASSIGN))
                        throw new SyntaxError(part[1].Start, part[1].End, "Unexpected token.");

                    var tokens = part.GetRange(2..);

                    if (tokens.Count == 0)
                        throw new SyntaxError(0, 0, "Missing value.");

                    var valueExpression = ExpressionParser.Parse(tokens);

                    var member = new DynamiclyNamedMember(nameExpression, valueExpression);
                    members.Add(member);
                    break;
                }

                case SymbolToken(Symbol.UNPACK_STRUCT):
                {
                    var tokens = part.GetRange(1..);

                    if (tokens.Count == 0)
                        continue;

                    var expression = ExpressionParser.Parse(tokens);
                    var member = new UnpackedMember(expression);
                    members.Add(member);
                    break;
                }

                default:
                    if (!part.Any(x => x is SymbolToken(Symbol.ASSIGN)))
                        throw new SyntaxError(0, 0, "Literal is ambiguous between an array and a struct.");
                    else
                        throw new SyntaxError(part[0].Start, part[^1].End, "Unexpected token.");
            }
        }

        return new StructLiteral(members);
    }
}