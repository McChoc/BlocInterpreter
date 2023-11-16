using Bloc.Core;
using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Variables;

namespace Bloc.Identifiers;

internal interface IIdentifier
{
    Value From(Module module, Call call);
    IValue Define(Value value, Call call, bool mask = false, bool mutable = true, VariableScope scope = VariableScope.Local);
}