using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Statements;

namespace Bloc.Cases;

internal abstract record Case(IExpression Expression, Statement Statement)
{
    public abstract CaseInfo Evaluate(Call call);
}