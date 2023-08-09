using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Values.Behaviors;

internal interface IIndexable
{
    IValue Index(Value value, Call call);
}