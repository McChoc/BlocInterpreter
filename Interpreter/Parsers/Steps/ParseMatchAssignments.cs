using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Expressions.Operators;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers.Steps;

internal sealed class ParseMatchAssignments : ParsingStep
{
    public ParseMatchAssignments(ParsingStep? nextStep)
        : base(nextStep) { }

    internal override IExpression Parse(List<Token> tokens)
    {
        for (var i = tokens.Count - 1; i >= 0; i--)
        {
            if (tokens[i] is SymbolToken(Symbol.MATCH_ASSIGN) @operator)
            {
                if (i > tokens.Count - 1)
                    throw new SyntaxError(@operator.Start, @operator.End, "Missing the right part of match assignment");

                var left = i > 0 ? Parse(tokens.GetRange(..i)) : null;
                var right = NextStep!.Parse(tokens.GetRange((i + 1)..));

                return new MatchAssignmentOperator(left, right);
            }
        }

        return NextStep!.Parse(tokens);
    }
}