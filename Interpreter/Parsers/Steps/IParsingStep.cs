using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Tokens;

namespace Bloc.Parsers.Steps;

internal interface IParsingStep
{
    IParsingStep? NextStep { get; init; }

    IExpression Parse(List<Token> tokens);
}