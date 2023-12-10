using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Statements;

namespace Bloc.Cases;

internal sealed record MatchCase(IExpression Expression, Statement Statement)
    : Case(Expression, Statement)
{
    public override CaseInfo Evaluate(Call call)
    {
        var value = Expression.Evaluate(call).Value;
        return new MatchCaseInfo(value, Statement);
    }
}