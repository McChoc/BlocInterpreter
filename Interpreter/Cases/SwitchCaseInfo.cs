using Bloc.Memory;
using Bloc.Statements;
using Bloc.Values.Core;

namespace Bloc.Cases;

internal sealed record SwitchCaseInfo(Value Value, Statement Statement)
    : CaseInfo(Value, Statement)
{
    public override bool Matches(Value value, Call call)
    {
        return Value.Equals(value);
    }
}