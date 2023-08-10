using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseBooleanXORs : ParsingStep
{
    public ParseBooleanXORs(ParsingStep? nextStep)
        : base(nextStep) { }

    internal override IExpression Parse(List<Token> tokens)
    {
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            if (tokens[i] is SymbolToken(Symbol.BOOL_XOR) @operator)
            {
                if (i == 0)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the left part of logical XOR");

                if (i == tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of logical XOR");

                var left = Parse(tokens.GetRange(..i));
                var right = NextStep!.Parse(tokens.GetRange((i + 1)..));

                return new BooleanXorOperator(left, right);
            }
        }

        return NextStep!.Parse(tokens);
    }
}