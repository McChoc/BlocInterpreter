using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Tokens;

namespace Bloc.Parsers.Steps;

internal abstract class ParsingStep
{
    protected ParsingStep? NextStep { get; }

    protected ParsingStep(ParsingStep? nextStep)
    {
        NextStep = nextStep;
    }

    internal abstract IExpression Parse(List<Token> tokens);
}