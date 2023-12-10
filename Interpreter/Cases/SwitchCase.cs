using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Statements;

namespace Bloc.Cases;

internal sealed record SwitchCase(IExpression Expression, Statement Statement)
    : Case(Expression, Statement)
{
    public override CaseInfo Evaluate(Call call)
    {
        var value = Expression.Evaluate(call).Value;
        return new SwitchCaseInfo(value, Statement);
    }
}