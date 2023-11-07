using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Tokens;

namespace Bloc.Parsers.Steps;

internal interface IParsingStep
{
    IExpression Parse(List<IToken> tokens);
}