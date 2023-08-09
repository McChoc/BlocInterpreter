using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Statements.Arms;

internal interface IArm
{
    Statement Statement { get; }

    bool Matches(Value value, Call call);
}