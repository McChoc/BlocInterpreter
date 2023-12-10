using Bloc.Memory;
using Bloc.Statements;
using Bloc.Values.Core;

namespace Bloc.Cases;

internal abstract record CaseInfo(Value Value, Statement Statement)
{
    public int JumpCount { get; set; }

    public abstract bool Matches(Value value, Call call);
}