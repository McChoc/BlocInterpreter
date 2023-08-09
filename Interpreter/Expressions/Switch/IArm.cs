using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Expressions.Switch;

internal interface IArm
{
    IExpression Expression { get; }

    bool Matches(Value value, Call call);
}